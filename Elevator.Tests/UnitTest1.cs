using NUnit.Framework;
using ElevatorChallenge;

namespace Elevator.Tests;

[TestFixture]
[Description("The Elevator exposes methods to check its current weight and current floor to be used in assertions")]
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
        // max weight prevents elevator from pathing to 5
        Assert.True(_testElevator.getCurrentFloor() == 4);
        Assert.True(_testElevator.getWeight() == 3);
        _testElevator.handleInput("3");
        Thread.Sleep(500);
        Assert.True(_testElevator.getCurrentFloor() == 3);
        Assert.True(_testElevator.getWeight() == 2);
        Thread.Sleep(750);
        Assert.True(_testElevator.getCurrentFloor() == 5);
        Assert.True(_testElevator.getWeight() == 3);
    }

    [Test]
    [Description("Demonstrates the elevator does not add new inputs after receiving Q")]
    public void StopsObeyingCommandsAfterQuit()
    {
        _testElevator.handleInput("4D");
        Thread.Sleep(1100);
        Assert.True(_testElevator.getCurrentFloor() == 4);
        Assert.True(_testElevator.getWeight() == 1);
        _testElevator.handleInput("2");
        _testElevator.handleInput("Q");
        Thread.Sleep(350);
        // Assert it keeps moving down towards floor 2 where the occupant wishes to exit, though a quit has been received
        Assert.True(_testElevator.getCurrentFloor() != 4);
        _testElevator.handleInput("1U");
        Assert.True(_testElevator.getWeight() == 1);
        // Assert that new inputs do not affect the elevator after the Q (the 1 Request is the person who got on at floor 4, who's still working on leaving)
        Assert.True(_testElevator.getRequests() == 1);
    }


}