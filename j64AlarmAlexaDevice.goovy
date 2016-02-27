/**
 *  j64 - Alexa Switch
 *
 *  Copyright 2016 joejarvis64@gmail.com
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
 */
metadata {
	definition (name: "j64 - Alexa Switch", namespace: "j64", author: "joejarvis64@gmail.com") {
		capability "Switch"
	}

	simulator {
		// TODO: define status and reply messages here
	}

    tiles {
        standardTile("switchTile", "device.switch", width: 2, height: 2, canChangeIcon: true) {
            state "off", label: '${name}', action: "switch.on", icon: "st.switches.switch.off", backgroundColor: "#ffffff"
            state "on", label: 'SHM-Stay', action: "switch.off", icon: "st.switches.switch.on",  backgroundColor: "#E60000"
        }

        standardTile("refreshTile", "device.power", decoration: "ring") {
            state "default", label:'', action:"refresh.refresh", icon:"st.secondary.refresh"
        }

        main "switchTile"
            details(["switchTile","refreshTile"])}
	}

// parse events into attributes
def parse(String description) {
	log.debug "Parsing '${description}'"
	// TODO: handle 'switch' attribute

}

// handle commands
def on() {
	log.debug "Executing 'on' - set Smart Home Monitor to stay mode"
	setShmAlarmMode("stay");
}

def off() {
	log.debug "Executing 'off' - set Smart Home Monitor to off"
	setShmAlarmMode("off");
}

private def setShmAlarmMode(name) {
    def event = [
        name:           "alarmSystemStatus",
        value:          name,
        isStateChange:  true,
        displayed:      true,
        description:    "alarm system status is ${name}",
    ]
    sendEvent(event)
}
