var inputs;
var upper_total_p;
var total_p;
var boxset_p;
var rollboxes;
var actionboxes;

var upper_total = 0;
var total = 0;
var boxset = 0;
var step = 0;
var round = 0;

function init()
{
	inputs = document.getElementsByClassName("scorebox");

	for (var i = 0; i < inputs.length; i++)
	{
		inputs[i].onchange = onScoresChanged;
	};

	upper_total_p = document.getElementById("uppertotal");
	total_p = document.getElementById("total");
	boxset_p = document.getElementById("boxset");

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

	var step_p = document.getElementById("step");
	step_p.innerHTML = "Round " + (round + 1);
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

	boxset_p.innerHTML = "Boxset: " + boxset;
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

	actionboxes[index].innerHTML = "YAAA " + str;
}

init();