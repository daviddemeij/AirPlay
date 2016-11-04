//This games side server should be run on the computer were the visualizations will be shown
//it should connect to a javascript-node udp sender to get information
//the visualization has to connect as a client in order to get the information

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


////////// Listens to the kinectRigBridge node js forwarder//////
var PORT = 9003;
//var TRACKERHOST = '130.89.243.9';
var TRACKERHOST = 'Z97USB3';


var dgram = require('dgram');
var udplistner = dgram.createSocket('udp4');

//for debugging / checking update speed
var currentTime =(new Date()).getTime();
var lastTime = currentTime;
var currentTimeClient = (new Date()).getTime();
var lastTimeClient = currentTimeClient;

udplistner.on('listening', function () {
    var address = udplistner.address();
    console.log('AAP UDP udplistner listening on ' + address.address + ":" + address.port);
});

//handle the incoming message from the kinectRigBridge node js forwarder
udplistner.on('message', function (message, remote) {
	//for checking/debugging only
	
	currentTime =(new Date()).getTime();
	console.log(currentTime-lastTime + "\t" +  message );
	lastTime = currentTime;
	
	
	//send to all local visualization clients over the TCP server
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
});

function removeClient(c) {
  console.log("Client Lost");
  clients.splice(clients.indexOf(c), 1);
}

udplistner.on('error', function (err) {
	console.log('UDP error '+ err);
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
