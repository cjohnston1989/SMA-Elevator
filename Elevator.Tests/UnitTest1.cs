using NUnit.Framework;
using ElevatorChallenge;

namespace Elevator.Tests;

[TestFixture]
public class Tests
{
    private ElevatorChallenge.Elevator _testElevator;
    [SetUp]
    public void Setup()
    {
        _testElevator = new ElevatorChallenge.Elevator(mockLog: true, runspeed: 10);
    }

    [Test]
    [Description("Simply demonstrates that someone on floor two can press the button and the elevator will move to them and pick them up.")]
    public void MovesToPickSomeoneUp()
    {
        _testElevator.handleInput("2U");
        Thread.Sleep(450);
        Assert.True(_testElevator.getCurrentFloor() == 2);
        Assert.True(_testElevator.getWeight() == 1);
    }

    [Test]
    [Description("Demonstrates the elevator will move to a floor to pick someone up, and then move to the floor the picked up person indicates to let them off.")]
    public void MovesToDropSomeoneOff()
    {
        _testElevator.handleInput("2U");
        Thread.Sleep(450);
        Assert.True(_testElevator.getWeight() == 1);
        _testElevator.handleInput("1");
        Thread.Sleep(450);
        Assert.True(_testElevator.getCurrentFloor() == 1);
        Assert.True(_testElevator.getWeight() == 0);
    }

    [Test]
    [Description("Demonstrates the elevator's weight limit's effect on pickup strategy, and also demonstrates the updating of target floor when a new request comes in 'on the way' towards the target")]
    public void WaitsAtMaxWeightToPickUp()
    {
        _testElevator.handleInput("2U");
        _testElevator.handleInput("3U");
        _testElevator.handleInput("4U");
        _testElevator.handleInput("5U");
        Thread.Sleep(1350);
        Assert.True(_testElevator.getCurrentFloor() == 4);
        Assert.True(_testElevator.getWeight() == 3);
        Thread.Sleep(450);
        Assert.True(_testElevator.getCurrentFloor() == 4);
        Assert.True(_testElevator.getWeight() == 3);
        _testElevator.handleInput("3");
        Thread.Sleep(450);
        Assert.True(_testElevator.getCurrentFloor() == 3);
        Assert.True(_testElevator.getWeight() == 2);
        Thread.Sleep(750);
        Assert.True(_testElevator.getCurrentFloor() == 5);
        Assert.True(_testElevator.getWeight() == 3);
    }
}