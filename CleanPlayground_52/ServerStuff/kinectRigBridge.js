var dgram = require('dgram');
var clients = [];
var lastTime = 0;
var currentTime =0;

////////// dgram forward connects to tracker
//"client" connected to the other machine
var dgramForward = require('dgram');
var PORT = 9003;
//use alias of Unity PC as ip adress is not that static
//if you need to test or develop on a different machine change this value before running it
//var GAMEHOST = "130.89.243.9"; //tracker pc
//var GAMEHOST = "130.89.243.222"; //UNITY PC
var GAMEHOST = "Z97R9";
var message = new Buffer('Start message');

//gamesidesocket will be a socket that binds to the gamepc
var gameSideSocket = dgramForward.createSocket('udp4');

//TEST incoming connection and message (should only send once!)
gameSideSocket.on("message", function (msg, rinfo) {
  console.log("gameSideSocket got: " + msg + " from " + rinfo.address + ":" + rinfo.port);
});
////end of test

////////// src listens to tracker
var src = dgram.createSocket('udp4');

src.on("listening", function() {
  console.log("- Started Forwarding Info-");
});

src.on('error', function (err) {
	console.log('UDP error '+err);
});

src.on("message", function(buf, rinfo) {
 	//line ending seems to be in there allready
	message = new Buffer(buf.toString());	

	try {
		gameSideSocket.send(message, 0, message.length, PORT, GAMEHOST, function(err, bytes) {
			console.log('sent message \t' + message.toString());
		});
	} catch(err) {
		gameSideSocket.close();
		gameSideSocket = dgramForward.createSocket('udp4');
		console.log('error in send closed socket ' +err);
	}
	
});

// bind to tracker
src.bind(9000);

process.on('uncaughtException', function (err) {
	console.log('UDP uncaught exception '+err);
});

//PORTS
//9009 tracker <> forwarder
//9003 forwarde <> gamesidenode
//9000 gamesidenode <> game
