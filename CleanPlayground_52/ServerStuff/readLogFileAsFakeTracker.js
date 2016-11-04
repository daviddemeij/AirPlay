//This games side server should be run on the computer were the visualizations will be shown
//it should connect to a javascript-node udp sender to get information
//the visualization has to connect as a client in order to get the information

//OFFLINE, use a logfile as input
//var filename = './logfolder/playerPositionsSession_positions_4_15_9_42_36.txt';
var filename = './playerPositionsSession_positions_4_15_9_42_36.txt';
var blockUntilGameConnects = false;

//SETTINGS FOR CONNECTION TO THE TRACKER PC OVER UDP:
var PORT = 9003;
//var TRACKERHOST = '130.89.243.9';
//ALIAS IS PREFFERED AS THEY HAVE NON STATIC IPS
var TRACKERHOST = 'Z97USB3';

//TCP/IP protocol FOR THE GAME to connect to
var net   = require('net');
var clients = [];
var clientconnected = false;
var linereadinginterval = 1; //20

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
		clientconnected = true;
	} catch (err) {
		console.log("client had error during connection: " + err);
		//removeClient(client);
	}
	//will listen to all incoming connections
}).listen(9009, "0.0.0.0");

////OPEN A LOG FILE
//// uses the liner module in liner.js from 
//https://strongloop.com/strongblog/practical-examples-of-the-new-node-js-streams-api/
var fs = require('fs')
var liner = require('./liner')
var source = fs.createReadStream(filename)
source.pipe(liner)
liner.on('readable', function () {
     var line;
	 var fields;
	 var infochunk;
	 var nextinfochunck;
	 var id;
	 var intlastid = parseInt(0);
	 var intid = parseInt(0);
	 var remaininginfochunk = '';
	 
	 var inttempid = parseInt(0);
	 var x;
	 var y;
	 var z;
	 
     //while (line = liner.read()) {
     //     console.log(line);// do something with line
     //}
	 
	 //TODO actually use the time passed to parse a new line, now we use a standard interval
	setInterval( function() {
		//to unblock we simply set client as if it is connected, this is the only place where it is used
		if (!blockUntilGameConnects)
			clientconnected = true;
		
		//loop over all the ids for each timeslot
		//we check this by ids becoming bigger as they were send in this fashion in unity
			
		while (clientconnected && intlastid <= intid )
		{
			//if we have looped the ids sent the remaining message that was on hold
			if (remaininginfochunk) {
				sendStringLineToClient(infochunk);			
			}
			
			if (line = liner.read())
			{
				intlastid = intid;
				fields = line.split(/,/)
				//check for undefined to see if it has all information to send
				if ( typeof fields[1] !== 'undefined' && fields[1] &&
					typeof fields[2] !== 'undefined' && fields[2] &&
					typeof fields[3] !== 'undefined' && fields[3] &&
					typeof fields[4] !== 'undefined' && fields[4])
				{
					//do stuff if query is defined and not null
				
					id = fields[1];
					x = parseInt(parseFloat((fields[2])*10000)).toString();
					y = parseInt(parseFloat((fields[3])*10000)).toString();
					z = parseInt(parseFloat((fields[4])*10000)).toString();
					infochunk = id.concat(',').concat(x).concat(',').concat(y).concat(',').concat(z);
					
									
					inttempid = parseInt(id);
					if (inttempid!=NaN)
						intid = inttempid;
					else
						intid = parseInt(0);
						
					if( intid < intlastid)
					{
						remaininginfochunk = infochunk;	
					}
					else
					{
						sendStringLineToClient(infochunk);
						remaininginfochunk = '';
					}	
				}
			}
		}
		intlastid = 0;
		intid = 0;
	}
	, linereadinginterval);
})

function sendStringLineToClient(message) {
	if (true)
	{ 
		//show the message in the console, so we can see what the protocol and values are
		console.log(message);
	}

	//probably this makes no sense as other computers shouldn't connect over TCP/IP to this script
	//send this message to all visualization clients connected to our TCP server
	clients.forEach(function (client) {
		try {
			if (client.writable) {
				//id x y z
				client.write(message+"\n");				
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

//////////In offline mode we do not Listen to the kinectRigBridge node js forwarder//////
//for debugging / checking update speed
var currentTime =(new Date()).getTime();
var lastTime = currentTime;
var currentTimeClient = (new Date()).getTime();
var lastTimeClient = currentTimeClient;

/*
setInterval( function() { 
	var stringMsg = 'login';
	var message = new Buffer(stringMsg) ;
	console.log('re-sent message \t' + message.toString());
}
, 1000);
*/

//METHOD TO SEND INFO TO CLIENT
/* if (debug)
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
 */
 
function removeClient(c) {
  console.log("Client Lost");
  clients.splice(clients.indexOf(c), 1);
}

process.on('uncaughtException', function (err) {
	console.log('UDP uncaught exception '+err);
});

//PORTS
//9009 tracker <> forwarder
//9003 forwarde <> gamesidenode
//9000 gamesidenode <> game
