/**
 *  j64Alarm
 *
 *  Copyright 2016 Joe Jarvis
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 *  in compliance with the License. You may obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
 *  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License
 *  for the specific language governing permissions and limitations under the License.
 *
 *  Last Updated : 1/15/2016
 *
 */
definition(
	name: "j64 Alarm",
	namespace: "j64",
	author: "Joe Jarvis",
	description: "Control your envisalink alarm using j64 Alarm Server",
	category: "SmartThings Labs",
        iconUrl: "http://cnn.com/xyz.gif",
        iconX2Url: "http://cnn.com/xyz.gif",
        iconX3Url: "http://cnn.com/xyz.gif"
)

mappings {
    path("/installDevices") {
        action: [
            POST: "installDevices",
        ]
    }
    path("/UpdatePartition") {
        action: [
            POST: "updatePartition"
        ]
    }
    path("/UpdateZone") {
        action: [
            POST: "updateZone"
        ]
    }
}

/* *************** */
/* Install Methods */
/* *************** */
def installDevices() {
    state.j64Server = params.j64Server
    state.j64Port = params.j64Port
    hubApiGet("/api/AlarmSystem")
}

def installAllDevices(partitions, zones) {
	def children = getChildDevices()

	// Add any partitions that have not already been created
	partitions.each { p -> 
		def networkId = "partition" + p.Id
        def partitionDevice = children.find { item -> item.device.deviceNetworkId == networkId }
        if (partitionDevice == null) {
	        def name = "j64:" + p.Name + " Partition"
        	log.debug "Add Partition: ${name} => ${networkId}"
	        partitionDevice = addChildDevice("j64", "j64 Partition", networkId, null, [name: "${name}", label:"${name}"])
		}
            
        // always set the current status for the partition
        partitionDevice.setAlarm(p.InAlarm, p.IsArmed)
        partitionDevice.setMode(p.ArmingMode, p.ReadyToArm)
	}
    
    zones.each { z ->
		def networkId = "zone" + z.Id
        def zoneDevice = children.find { item -> item.device.deviceNetworkId == networkId }
        if (zoneDevice == null) {
	        def name = "j64:" + z.Name
            def zoneType = "j64 Contact Zone"
            if ( z.ZoneType == 1 )
               zoneType = "j64 Motion Zone"
               
        	log.debug "Add Zone: ${name} => ${networkId}"
	        zoneDevice = addChildDevice("j64", zoneType, networkId, null, [name: "${name}", label:"${name}"])
        }    
        
        // set the current status for the zone
        zoneDevice.setState(z.Status)
    }
}

/* ***************** */
/* Partition Methods */
/* ***************** */
def updatePartition(evt) {
    def Id = params.Id
	def Name = params.Name
	def ReadyToArm = params.ReadyToArm
    def IsArmed = params.IsArmed
    def AlarmOn = params.AlarmOn
    def ArmingMode = params.ArmingMode

	def children = getChildDevices()
    def networkId = "partition${Id}"
    def partitionDevice = children.find { item -> item.device.deviceNetworkId == networkId }
    if (partitionDevice != null) {
		log.debug "updatePartition: ${Id} ${Name} ${ReadyToArm} ${IsArmed} ${AlarmOn} ${ArmingMode}"
		partitionDevice.setAlarm(AlarmOn, IsArmed)
        partitionDevice.setMode(ArmingMode, ReadyToArm)
        
        // Keep smart home monitor (SHM) in synch with the j64 alarm server
        // We sync SHM with partition 1 in the alarm system since it does
        // not support multiple partitions
        if (Id == 1) {
        	if ("${IsArmed}".toLowerCase() == "false") {
				setAlarmMode("off")
			} else {
        		if (ArmingMode == "Away")
            		setAlarmMode("away")
        	    else
    	        	setAlarmMode("stay")
	        }
        }
	}
}

def armPartition(partitionId, stayAway) {
	if (stayAway == "away")
	    hubApiGet("/api/AlarmSystem/AwayArm/${partitionId}")
	else
    	hubApiGet("/api/AlarmSystem/StayArm/${partitionId}")
}

def disarmPartition(partitionId) {
   	hubApiGet("/api/AlarmSystem/Disarm/${partitionId}")
}

def refreshPartition(partitionId) {
    hubApiGet("/api/AlarmSystem")
}

/* ************ */
/* Zone Methods */
/* ************ */
def updateZone(evt) {
    def Id = params.Id
    def Name = params.Name
	def Status = params.Status
    
	def children = getChildDevices()
    def networkId = "zone${Id}"
    def zoneDevice = children.find { item -> item.device.deviceNetworkId == networkId }
    if (zoneDevice != null) {
		log.debug "updateZone: ${Id} ${Name}->${Status}"
    	zoneDevice.setState(Status)
	}
}

def refreshZone(zoneId) {
    hubApiGet("/api/AlarmSystem")
}

/* ************** */
/* Initialization */
/* ************** */
def installed() { 
	initialize() 
}

def updated() { 
	unsubscribe()
  	unschedule()
	initialize() 
}

def uninstalled() {
	unschedule()
}	

def initialize() {
	subscribe(location, null, localLanHandler, [filterEvents:false])
    subscribe(location, "alarmSystemStatus", shmModeChangeHandler, [filterEvents:false])
}

def refresh() {   
    hubApiGet("/api/AlarmSystem")
}

/* ************************************ */
/* Handle event from the j64AlarmServer */ 
/* on the local LAN                     */
/* ************************************ */
def localLanHandler(evt) {
	// Only handle messages from the j64AlarmServer
	def msg = parseLanMessage(evt.description)
	if (msg.json == null) {
	    return
    }
    
    def FromHost = msg.json.FromHost
    def Route = msg.json.Route
    
    if (FromHost != j64AlarmServerAddress()) {
	    return
    }
    
    if (Route == "/api/AlarmSystem")
       installAllDevices(msg.json.Response.Partitions, msg.json.Response.Zones)
}

def j64AlarmServerAddress() {
	return state.j64Server + ":" + state.j64Port
}

/* ********************************** */
/* Handle event when the mode changes */
/* ********************************** */
def shmModeChangeHandler(evt) {

	// Unfortunately we have to assume partition 1 for the smart home monitor since it does not support
    // multiple partitions.  Maybe this could change based on the location??
	def Id = 1
	def networkId = "partition${Id}"

	def partitionDevice = getChildDevices().find { item -> item.device.deviceNetworkId == networkId }
    if (partitionDevice != null) {
    	def mode = partitionDevice.currentAlarm 

		if (evt.value == "off" && mode == "armed") {
	    	disarmPartition(1)
    	}
        
        if (evt.value != "off" && mode == "disarmed") {
	    	armPartition(1, evt.value)
    	}
	}
}

private def setAlarmMode(name) {
    def event = [
        name:           "alarmSystemStatus",
        value:          name,
        isStateChange:  true,
        displayed:      true,
        description:    "alarm system status is ${name}",
    ]
    sendLocationEvent(event)
}

/* **************** */
/* Helper Functions */
/* **************** */
private hubApiGet(apiPath) {	

    def result = new physicalgraph.device.HubAction(
 		   	method: "GET",
    		path: apiPath,
    		headers: [
        		HOST: j64AlarmServerAddress()
    		]
		)
    sendHubCommand(result)
}