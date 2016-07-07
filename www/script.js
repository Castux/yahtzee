var inputs;
var upper_total_p;
var total_p;
var rollboxes;
var actionboxes;

var upper_total = 0;
var total = 0;
var boxset = 0;
var round = 0;

var rollIndices;

function init()
{
	inputs = document.getElementsByClassName("scorebox");

	for (var i = 0; i < inputs.length; i++)
	{
		inputs[i].onchange = onScoresChanged;
	};

	upper_total_p = document.getElementById("uppertotal");
	total_p = document.getElementById("total");

	rollboxes = document.getElementsByClassName("rollbox");
	for (var i = 0; i < rollboxes.length; i++)
	{
		let j = i;
		rollboxes[i].onchange = function()
		{
			onRollboxChanged(j);
		}
	};

	actionboxes = document.getElementsByClassName("actionbox");

	onScoresChanged();

	rollIndices = {};
	for (var i = 0; i < rolls.length; i++)
	{
		rollIndices[rolls[i]] = i;
	};
}

function onScoresChanged()
{
	upper_total = 0;
	total = 0;
	round = 0;

	// Up

	for (var i = 0; i < inputs.length; i++)
	{
		var value = inputs[i].value;
		if(value != "")
		{
			var number = parseInt(value);
			total += number;

			if(i < 6)
				upper_total += number;

			round++;
		}
	};

	// Display

	upper_total_p.innerHTML = "Upper total: " + upper_total;

	if(upper_total >= 63)
	{
		total += 35;
		upper_total_p.innerHTML += " (+35)";
	}

	total_p.innerHTML = "Total: " + total;

	computeBoxset();

	var round_p = document.getElementById("step");
	round_p.innerHTML = "Round " + (round + 1);
}

function computeBoxset()
{
	boxset = 0;

	for (var i = 0; i < inputs.length; i++)
	{
		var value = inputs[i].value;
		if(value == "")
		{
			boxset += (1 << i);
		}
	}
}

function onRollboxChanged(index)
{
	var str = rollboxes[index].value.split("");

	if(str.length != 5)
	{
		rollboxes[index].value = "";
		return;
	}

	for (var i = 0; i < str.length; i++)
	{
		var num = parseInt(str[i]);
		if(num < 0 || num > 6)
		{
			rollboxes[index].value = "";
			return;
		}

		str[i] = num;
	};

	str.sort();
	str = str.join("");
	rollboxes[index].value = str;

	// trigger data fetch

	var step = round * 3 + index;

	fetchAction(step, str, function(action)
	{
		actionboxes[index].innerHTML = action;
	});
}

function fetchAction(step, roll, cb)
{
	var url = "data/step" + step + "/data" + boxset;
	fetchURL(url, function(txt)
	{
		var arr = JSON.parse("[" + txt.slice(0,-1) + "]");

		var index = (upper_total * rolls.length + rollIndices[roll]) * 2;
		var action = arr[index];
		var value = arr[index + 1];

		var phase = step % 3;
		if(phase == 2)
		{
			action = "Score " + boxNames[action];
		}
		else
		{
			action = "Keep " + applyKeepPattern(roll, action);
		}

		cb(action + " (" + value + ")");
	});
}

function fetchURL(url, cb)
{
	var xmlhttp = new XMLHttpRequest();
	
	xmlhttp.onreadystatechange = function()
	{
		if (xmlhttp.readyState == 4 && xmlhttp.status == 200)
		{
			cb(xmlhttp.responseText);
		}
	};

	xmlhttp.open("GET", url, true);
	xmlhttp.send();
}

function applyKeepPattern(roll, pattern)
{
	var keep = "";

	for (var i = 0; i < 5; i++)
	{
		if(pattern & (1 << i))
			keep += roll[i];
	}

	return keep;
}

init();