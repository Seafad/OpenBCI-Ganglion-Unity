const net = require('net');
const Ganglion = require("@openbci/ganglion");
const ganglion = new Ganglion();

//Server
var host = '127.0.0.1';
var port = 8080;
var socket;
var DataType = {StatusOk: 0, EEG: 1, Impedance: 2, GanglionInfo: 3, Message: 8, Error: 9};
var foundGanglions = [];
var startupError;

net.createServer(function(sock) {
	socket = sock;
    log('Client connected: ' + socket.remoteAddress +':'+ socket.remoteport);
	
	socket.on('error', function (err) {
	if(err.code == "EPIPE"){
		socket.end();
		return;
	}
	if(socket){
		sendData(DataType.Error, err.toString());
	}else{
		startupError = err.toString();
	}
	});
	
    socket.on('data', function(data) {
		let strData = data.toString();
        processCommand(strData);
    });

    socket.on('close', function(data) {
		if(ganglion.isConnected()){
			log("Disconnecting on close");
			ganglion.disconnect(true);
		}
        log('Socket closed: ' + socket.remoteAddress +' '+ socket.remotePort);
    });
	
	
	if(startupError){
		sendData(DataType.Error, startupError);
		return;
	}
	for(let i = 0; i < foundGanglions.length; i++){
		log("Send data for Ganglion " + (i + 1) + ':\n' + JSON.stringify(foundGanglions[i]));
		sendData(DataType.GanglionInfo, foundGanglions[i]);
	}
}).listen(port, host);

log('Server listening on ' + host +':'+ port);

function sendData(dataType, data){
	if(!socket)
		return;
	if(data == null){
		socket.write(dataType.toString())
	}else{
	socket.write(dataType + '|' + JSON.stringify(data));
	}
}

function log(message){
	console.log('[' + new Date().toLocaleTimeString() + ']: ' + message )
}


process.on('uncaughtException', function (err) {
	log(err);
	if(socket){
		sendData(DataType.Error, err.toString());
	}else{
		startupError = err.toString();
	}
});

//Ganglion
ganglion.once("ganglionFound", peripheral => {
		foundGanglions.push(
			{
				id: peripheral.id,
				name: peripheral.advertisement.localName, 
				state: peripheral.state, 
				connectable: peripheral.connectable
			});
		if(socket){
			for(let i = 0; i < foundGanglions.length; i++){
				sendData(DataType.GanglionInfo, foundGanglions[i]);
			}
		}
		log("New peripheral found " + peripheral.advertisement.localName);
		ganglion.searchStop();
});

ganglion.on('rawDataPacket', packet => {
	log("RAW: " + packet);
});

ganglion.on('message', message =>{
	sendData(DataType.Message, message);
});

ganglion.on("sample", sample => {
	if(sample.valid){
		if(sample.accelDataCounts){
			sendData(DataType.EEG, {
				sampleNumber: sample.sampleNumber,
				channelData: sample.channelData,
				accelData: sample.accelDataCounts
			});
		}else{
			sendData(DataType.EEG, {
			sampleNumber: sample.sampleNumber,
			channelData: sample.channelData
			});
		}
	}
});
/*
ganglion.on('accelerometer', accel => {
	sendData(DataType.Accelerometer, accel);
});
*/

ganglion.on('impedance', impedance => {
	sendData(DataType.Impedance, impedance);
});


function processCommand(strData){
	if(!ganglion.isNobleReady()){
		sendData(DataType.Error, 'Error: No compatible USB Bluetooth 4.0 device found!');
		return;
	}
	
	let command = strData[0];
	log("Received Command: " + command);
	
	switch(command){
		case 'i':	{
			log("Starting search");
			peripherals = [];
			ganglion.searchStart(50000).then(null, fail);
			break;
		 }
		case 'e': {
			log("Stopping search");
			ganglion.searchStop().then(null, fail);
			break;
		 }
		case 'c': {
			let id = strData.substr(1, strData.length);
			log("Connecting to " + id + "...");
			ganglion.once("ready", () => {
				log("Ready.");
				sendData(DataType.StatusOk, null);
			});
			ganglion.connect(id).then(null, fail);
			break;
		}
		case 'd': {
			log("Disconnecting...");
			ganglion.disconnect(true).then(onDisconnected, fail);
			break;
		 } 
		case 'b': {
			log("Starting stream");
			if(ganglion.isConnected()){
				ganglion.streamStart().then(null, fail);
			}
			break;
		}
		case 's': {
			log("Stopping stream");
			if(ganglion.isConnected())
				ganglion.streamStop().then(null, fail);
			break;
		}

		case 'z':{
			log("Activating impedance test");
			ganglion.impedanceStart();
			break
		}
		case 'Z':{
			log("Deactivating impedance test");
			ganglion.impedanceStop();
			break
		}
		default:{
			ganglion.write(command).then(null, fail);
			break;
		}
	}; 	
};

function printRegisters(res){
	log("Resgisters: " + res);
}
function onDisconnected(){
	log("Disconnected.");
	sendData(DataType.StatusOk, null);
}

function fail(res){
	log("Operation failed: " + res.toString());
	sendData(DataType.Error, res.toString());
}
