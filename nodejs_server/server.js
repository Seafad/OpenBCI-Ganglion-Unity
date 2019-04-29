const net = require('net');
const Ganglion = require("@openbci/ganglion");
const ganglion = new Ganglion();

//Server
var host = '127.0.0.1';
var port = 8080;
var socket;
var DataType = {GanglionInfo: 0, EEG: 1, Error: 2};
var foundGanglions = {
			type: DataType.GanglionInfo,
			data: []
		};
var startupError;

net.createServer(function(sock) {
	socket = sock;
    console.log('Client connected: ' + socket.remoteAddress +':'+ socket.remoteport);
	
    socket.on('data', function(data) {
		let strData = data.toString();
        processCommand(strData);
    });

    socket.on('close', function(data) {
        console.log('CLOSED: ' + socket.remoteAddress +' '+ socket.remotePort);
    });
	
	if(startupError){
		socket.write(startupError);
		return;
	}
 
	if(foundGanglions.data.length > 0){
		console.log("Peripherals startup: " + JSON.stringify(foundGanglions));
		socket.write(JSON.stringify(foundGanglions));
	}

}).listen(port, host);

console.log('Server listening on ' + host +':'+ port)

ganglion.once("ganglionFound", peripheral => {
		foundGanglions.data.push(
			{
				id: peripheral.id,
				name: peripheral.advertisement.localName, 
				state: peripheral.state, 
				connectable: peripheral.connectable
			});
		if(socket){
			socket.write(JSON.stringify(foundGanglions));
		}
		console.log("Peripheral found: " + JSON.stringify(foundGanglions));
		ganglion.searchStop();
});


function processCommand(strData){
	if(!ganglion.isNobleReady()){
		socket.write(JSON.stringify({
		type: DataType.Error,
		data: 'Error: No compatible USB Bluetooth 4.0 device found!'
	}));
	return;
	}
	
	let command = strData[0];
	console.log("Received Command: " + command);
	
	 switch(command){
		 case 'b': {
			if(ganglion.isConnected()){
				ganglion.streamStart();
				ganglion.on("sample", sample => {
					socket.write(JSON.stringify({type: DataType.EEG, data: sample}));
				});
			}
			break;
			}
		 case 's': {
			 if(ganglion.isConnected())
				 ganglion.streamStop();
			 break;
			}
		 case 'i':	{
			peripherals = [];
			ganglion.searchStart(35000);
			break;
		 }
		 case 'e': {
			 ganglion.searchStop();
			 break;
		 }
		 case 'c': {
			 ganglion.connect(peripherals[0]);
			 ganglion.once("ready", () => {
			 });
			 break;
		 }
		 case 'd': {
			 ganglion.disconnect(true);
		 } 
		 default:{
			 ganglion.write(command);
		 }
	 }; 	
 };
 	
process.on('uncaughtException', function (err) {
  console.log(err);
  let error = JSON.stringify({
		type: DataType.Error,
		data: err.toString()
	});
  if(socket){
	socket.write(error);
  }else{
	 startupError = error;
  }
});

