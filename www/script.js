var inputs;
var upper_total_p;
var total_p;
var boxset_p;

var upper_total = 0;
var total = 0;
var boxset = 0;

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

	onScoresChanged();
}

function onScoresChanged()
{
	upper_total = 0;
	total = 0;

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

init();