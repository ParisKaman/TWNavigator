﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ExampleTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ExampleTestSimplePasses()
        {
            // Use the Assert class to test conditions
            Assert.That(true);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ExampleTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            bool no = false;
            Assert.That(no == false);
            yield return null;
        }
    }
}
