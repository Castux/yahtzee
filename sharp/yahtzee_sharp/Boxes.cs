using System;
using System.Collections.Generic;

public class Boxes
{
	public List<string> Names { private set; get; }

	private Dictionary<string, Box> boxes;

	public Boxes(List<string> boxNames)
	{
		Names = boxNames;
		boxes = new Dictionary<string, Box>();

		for (var i = 0; i < Names.Count; i++)
		{
			boxes[Names[i]] = new Box(1 << i);
		}
	}

	public Box GetBox(string name)
	{
		return boxes[name];
	}

	public string GetName(Box box)
	{
		foreach (var pair in boxes)
			if (pair.Value.bits == box.bits)
				return pair.Key;

		throw new Exception("Wat");
	}

	public IEnumerable<Box> AllBoxes
	{
		get { return boxes.Values; }
	}

	public int NumBoxes
	{
		get { return boxes.Count; }
	}

	public int NumBoxSets
	{
		get { return 1 << NumBoxes; }
	}

	public BoxSet EmptyBoxSet
	{
		get { return new BoxSet(0, this); }
	}

	public BoxSet FullBoxSet
	{
		get { return new BoxSet(NumBoxSets - 1, this); }
	}

	public IEnumerable<BoxSet> AllBoxSets
	{
		get
		{
			for (int i = 0; i < NumBoxSets; i++)
				yield return new BoxSet(i, this);
		}
	}

}

public struct Box
{
	public int bits;

	public Box(int bits)
	{
		this.bits = bits;
	}

	public int Index
	{
		get { return (int)Math.Log(bits, 2); }
	}
}

public struct BoxSet
{
	public Boxes boxes;
	public int bits;

	public BoxSet(int bits, Boxes boxes)
	{
		this.boxes = boxes;
		this.bits = bits;
	}

	public void Add(Box box)
	{
		bits |= box.bits;
	}

	public void Remove(Box box)
	{
		bits &= ~box.bits;
	}

	public bool Contains(Box box)
	{
		return (bits & box.bits) != 0;
	}

	public IEnumerable<Box> Contents
	{
		get
		{
			for (int i = 0; i < boxes.NumBoxes; i++)
			{
				var box = new Box(1 << i);
				if (Contains(box))
					yield return box;
			}
		}
	}

	public bool IsEmpty
	{
		get { return bits == 0; }
	}
}