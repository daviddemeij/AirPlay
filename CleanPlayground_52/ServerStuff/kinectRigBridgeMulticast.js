var dgram = require('dgram');
var clients = [];
var lastTime = 0;
var currentTime =0;

////////// dgram forward connects to tracker
//"client" connected to the other machine
var dgramForward = require('dgram');
var PORT = 9003;

var message = new Buffer('Start message');

//gamesidesocket will be a socket that binds to the gamepc
var gameSideSocket = dgramForward.createSocket('udp4');
var broadcastList = [];

//listen for incoming connections
//when the message is "login" they will be added to the broadcast list 
gameSideSocket.on("message", function (msg, rinfo) {
    console.log("the gameSideSocket got an incoming message *" + msg + "* from " + rinfo.address + ":" + rinfo.port);
	
	//test if it is a new connection or a wrongfully send message
	if (msg.toString() == 'login')
	{	
		//notify the client
		//client should deal with this message differently than with coordinates
		var echoMessage = new Buffer('added');
		var doesThisAddressExist = false;
		broadcastList.forEach(function (addressNr) {
			try {
				if (addressNr == rinfo.address) {
					console.log('allready in log');
					doesThisAddressExist = true;
				} else {
					//removeBroadcast(addressNr);
					//only on actual stopped client
					//console.log('not equal to this one');
				}
			} catch (err) {
				console.log("sending initial message to listenerner had error" + err);
				//removeBroadcast(addressNr);
			}
		});
		
		//if the address is not yet in the list welcome the new client
		if (doesThisAddressExist == false)
		{
			broadcastList.push(rinfo.address);
			gameSideSocket.send(echoMessage, 0, echoMessage.length, PORT, rinfo.address, function(err, bytes) {
				console.log('welcomed \t' + rinfo.address);
			});
		}
		else {
			//it might be a relogin attempt after the socket was closed at the game siide only 
			//so we do need to renotify the client that its is added, as it might require this (even standard protocol for now!)
			gameSideSocket.send(echoMessage, 0, echoMessage.length, PORT, rinfo.address, function(err, bytes) {
				console.log('re-welcomed \t' + rinfo.address);
			});
		}		
		//for debugging output to the console which addresses are in the list
		var teller = 0;
		broadcastList.forEach(function (addressNr) {
			try {
				console.log('nr ' + teller + ' ip is: \t' + addressNr);
				teller = teller +1;
			} catch (err) {
				console.log("looping through list had an error" + err);
			}
		});
	}
	else 
	{	
		//probably the user has made a mistake and acknowledges incoming messages, don't deal with those,
		//it will slow down the process as every frame all addresses will be checked etc.
		
		//if you want you could try to sent something for debugging your program, perhaps you didn't send a string 'login'?
		//var echoMessage = new Buffer('0 0 0 0 false');
		//gameSideSocket.send(echoMessage, 0, echoMessage.length, rinfo.port, rinfo.address, function(err, bytes) {
		//		console.log('a non inlog message received \t' + echoMessage.toString());
		//});
	}
});

//start listening and notify the console we are listening to possible 'listening games' that want to receive tracking data
gameSideSocket.on("listening", function() {
  console.log("Also start to listen on gameSideSocket");
});

//perhaps it can get closed?
gameSideSocket.on("close", function() {
  console.log("socket is closed");
});

////////// src listens to tracker
var src = dgram.createSocket('udp4');

src.on("listening", function() {
	console.log("- Started Forwarding Info To Multicast-");
	gameSideSocket.bind(9003);
});

src.on('error', function (err) {
	console.log('UDP error '+err);
});

//the object tracker c++ programm has sent information
src.on("message", function(buf, rinfo) {
 	message = new Buffer(buf.toString());	

	console.log('trying to sent message \t' + message.toString());
	
	//we try to send this information to all listening games
	//the socket is not really one socket it seems!
	//
	broadcastList.forEach(function (addressNr) {	
		try {
			gameSideSocket.send(message, 0, message.length, PORT, addressNr, function(err, bytes) {
				
			});
		} catch(err) {
			//I am unclear whether we should empty the list as well, currently it can only grow
			gameSideSocket.close();
			gameSideSocket = dgramForward.createSocket('udp4');
			console.log('error in send closed socket ' +err);
		}
	});
	
});

// bind to the tracker running on this pc
src.bind(9000);

//notify console of uncaughtExceptions of all kinds not only about udp
process.on('uncaughtException', function (err) {
	console.log('UDP uncaught exception '+err);
});

//PORTS
//9003 forwarde <> gamesidenode
//9000 gamesidenode <> game (in other nodejs file, over tcp/ip)
//9000 tracker <> forwarder (src-socket here, over udp)

//THIS SHOULD LONGER BE NEEDED:
//var GAMEHOST = 'Z97R9';
//previouly we would use alias of Unity PC as ip adress and this was not that static
//if you need to test or develop on a different machine change this value before running it
//var GAMEHOST = "130.89.243.9"; //tracker pc
//var GAMEHOST = "130.89.243.222"; //UNITY PC