using System;
using System.Collections.Generic;

public class Boxes
{
	public List<string> Names { private set; get; }

	private Dictionary<string, Box2> boxes;

	public Boxes(List<string> boxNames)
	{
		Names = boxNames;
		boxes = new Dictionary<string, Box2>();

		for (var i = 0; i < Names.Count; i++)
		{
			boxes[Names[i]] = new Box2(1 << i);
		}
	}

	public Box2 GetBox(string name)
	{
		return boxes[name];
	}

	public string GetName(Box2 box)
	{
		foreach (var pair in boxes)
			if (pair.Value.bits == box.bits)
				return pair.Key;

		throw new Exception("Wat");
	}

	public IEnumerable<Box2> AllBoxes
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

	public BoxSet2 EmptyBoxSet
	{
		get { return new BoxSet2(0, this); }
	}

	public IEnumerable<BoxSet2> AllBoxSets
	{
		get
		{
			for (int i = 0; i < NumBoxSets; i++)
				yield return new BoxSet2(i, this);
		}
	}

}

public struct Box2
{
	public int bits;

	public Box2(int bits)
	{
		this.bits = bits;
	}

	public int Index
	{
		get { return (int)Math.Log(bits, 2); }
	}
}

public struct BoxSet2
{
	public Boxes boxes;
	public int bits;

	public BoxSet2(int bits, Boxes boxes)
	{
		this.boxes = boxes;
		this.bits = bits;
	}

	public void Add(Box2 box)
	{
		bits |= box.bits;
	}

	public void Remove(Box2 box)
	{
		bits &= ~box.bits;
	}

	public bool Contains(Box2 box)
	{
		return (bits & box.bits) != 0;
	}

	public IEnumerable<Box2> Contents
	{
		get
		{
			for (int i = 0; i < boxes.NumBoxes; i++)
			{
				var box = new Box2(1 << i);
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