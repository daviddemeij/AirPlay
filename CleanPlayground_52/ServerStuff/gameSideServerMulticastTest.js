//This games side server should be run on the computer were the visualizations will be shown
//it should connect to a javascript-node udp sender to get information
//the visualization has to connect as a client in order to get the information

//SETTINGS FOR CONNECTION TO THE TRACKER PC OVER UDP:
var PORT = 9003;
//var TRACKERHOST = '130.89.243.9';
//ALIAS IS PREFFERED AS THEY HAVE NON STATIC IPS
var TRACKERHOST = 'Z97USB3';

//TCP/IP protocol FOR THE GAME to connect to
var net   = require('net');
var clients = [];

//creates a local TCP server for the visualization, e.g. a Unity game
//this function simply adds this client to a list of clients
net.createServer(function(socket) {
	//show in the console if the client (in this case the game) tries to connect
	console.log("Client tries to connect");
	try {
		//create a socket, 
		socket.name = socket.remoteAddress + ":" + socket.remotePort; 
		
		//according to documentation this is already the default setting!
		socket.setNoDelay= true;
		clients.push(socket);
		socket.on('end', function() {
			removeClient(socket);
		});
		//also remove client on error
		socket.on('error', function() {
			removeClient(socket);
		});
		console.log("Client is connected");
	} catch (err) {
		console.log("client had error during connection: " + err);
		//removeClient(client);
	}
	//will listen to all incoming connections
}).listen(9009, "0.0.0.0");


////////// Listen to the kinectRigBridge node js forwarder//////
var dgram = require('dgram');
var udplistner = dgram.createSocket('udp4');
var loggedIn = false;

//for debugging / checking update speed
var currentTime =(new Date()).getTime();
var lastTime = currentTime;
var currentTimeClient = (new Date()).getTime();
var lastTimeClient = currentTimeClient;
var finishedOnListening = false;

setInterval( function() { 
	if(!loggedIn && finishedOnListening)
	{
		var stringMsg = 'login';
		var message = new Buffer(stringMsg) ;
		udplistner.send(message, 0, message.length, PORT, TRACKERHOST, function(err, bytes) {
			//console.log('re-sent message \t' + message.toString());
		});
	}
}
, 1000);

//start listening and the same time request a 'login'
//the 'server' will add this address to its broadcast list
udplistner.on('listening', function () {
    var address = udplistner.address();
    console.log('UDP udplistner listening on ' + address.address + ":" + address.port);
    var stringMsg = 'login';
	var message = new Buffer(stringMsg) ;
	//notify the trackerhost and add me (this ip) to the broadcastlist
	udplistner.send(message, 0, message.length, PORT, TRACKERHOST, function(err, bytes) {
		console.log('sent message \t' + message.toString());
	});
	//if we havent logged in we need to know we at least tried once
	finishedOnListening = true;
		
});

//handle the incoming message from the kinectRigBridge node js forwarder
udplistner.on('message', function (message, remote) {
	
	var debug = true;
	if (debug)
	{
		//for checking/debugging only
		currentTime =(new Date()).getTime();
		console.log(currentTime-lastTime + "\t" +  message );
		lastTime = currentTime;
	}
	
	//if we have not yet been added to the trackernode's broadcast list check if this message was an added notification 
	if (!loggedIn && message.toString() == 'added')
	{
		loggedIn = true;
		if (debug)
		{ 
			console.log('logged in \t' + message);
		}
	}
	//if it is not an added notification it will be according to the protocol that the game will be able to deal with
	else
	{
		if (debug)
		{ 
			//show the message in the console, so we can see what the protocol and values are
			console.log(message.toString());
		}
		
		//probably this makes no sense as other computers shouldn't connect over TCP/IP to this script
		//send this message to all visualization clients connected to our TCP server
		clients.forEach(function (client) {
			try {
				if (client.writable) {
					//id x y z
					client.write(message.toString()+"\n");				
				} else {
					removeClient(client);
					//only on actual stopped client
					console.log('Client not writable and removed');
				}
			} catch (err) {
				console.log("client had error" + err);
				removeClient(client);
			}
		});
	}
});

function removeClient(c) {
  console.log("Client Lost");
  clients.splice(clients.indexOf(c), 1);
}

udplistner.on('error', function (err) {
	console.log('UDP error'+ err);
});

//
udplistner.bind(PORT);

process.on('uncaughtException', function (err) {
	console.log('UDP uncaught exception '+err);
});

//PORTS
//9009 tracker <> forwarder
//9003 forwarde <> gamesidenode
//9000 gamesidenode <> game
