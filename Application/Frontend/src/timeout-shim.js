// Modified version of https://gist.githubusercontent.com/ChadBurggraf/4c4abef515852aa1efee04478a8a8641/raw/8147e2c10567547b9c452382c3366352dc51cb1e/interval-timeout-shim.js

var objects = [];

var root = global;

function clearInterval(id) {
	clearObject.call(root, id);
}

function clearObject(id) {
	var i = objects.length;
	
	while (i--) {
		if (objects[i].id == id) {
			objects.splice(i, 1);
		}
	}
}

function clearTimeout(id) {
	clearObject.call(root, id);
}

function getTime() {
	return +(new Date());
}

function setInterval(callback, delay) {
	return setObject("interval", callback, delay);
}

function setObject(type, callback, delay) {
	var id = objects.length;

	objects.push({
		callback: callback,
		delay: delay,
		id: id,
		time: getTime(),
		type: type
	});

	return id;
}

function setTimeout(callback, delay) {
	return setObject("timeout", callback, delay);
}

function tick() {
	var time = getTime();
	var i = objects.length;

	while (i--) {
		var object = objects[i];

		if (time > object.time + object.delay) {
			object.callback();

			if (object.type === "timeout") {
				clearObject(object.id);
			} else {
				object.time = time;
			}
		}

		object = null;
	}

	i = null;
	time = null;
}

if (typeof root["clearInterval"] !== "function" || typeof root["setInterval"] !== "function") {
	root["clearInterval"] = clearInterval;
	root["setInterval"] = setInterval;
	setInterval.call(root, tick, 10);
}

if (typeof root["clearTimeout"] !== "function" || typeof root["setTimeout"] !== "function") {
	root["clearTimeout"] = clearTimeout;
	root["setTimeout"] = setTimeout;
}

export {
	setTimeout,
	clearTimeout,
}