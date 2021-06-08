using System;
using System.Collections.Generic;
using UnityEngine;

public class Step
{
    public Transform Target { get; set; }
    public GameObject Card { get; set; }

    internal List<Step> ToList()
    {
        List<Step> steps = new List<Step>();
        Step s = new Step();
        s.Card = Card;
        s.Target = Target;

        steps.Add(s);
        return steps;
    }
}

