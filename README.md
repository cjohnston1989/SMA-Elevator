# SMA-Elevator

Chris Johnston's solution to this Elevator Challenge.

# The Challenge

Create an application that simulates the operation of a simple elevator.

Requirements

The elevator must travel in one direction at a time until it needs to go no further (e.g. keep going until the elevator has reached the top/bottom of the building, or no stop is requested on any floor ahead).<br />
Elevator floor request buttons can be pressed asynchronously from inside or outside the elevator while it is running.<br />
Elevator will stop at the closest floor first, in the direction of motion, then the next closest and so on. Any floors requested while the elevator is moving should be taken into account.<br />
Elevator will stop at all asynchronously requested floors, only if the request is made while the elevator is at least one floor away (e.g. if elevator is between 4th and 5th floor, going up, and the 5th floor is requested at that moment, elevator will not stop at the 5th floor while going up; it will stop there while going down).<br />
When elevator arrives at a requested floor, it waits for 1 second. It takes 3 seconds to travel between consecutive floors.<br />
A sensor tells the elevator its direction, next/current floor, state (stopped, moving) and if the elevator has reached its max weight limit.<br />
Use the sensor data plus the asynchronous floor request button data to work the elevator.<br />
Write meaningful unit tests that show the elevator works correctly, even if the application is not run.<br />
Log the following to a file, to verify elevator works well:
* Timestamp and asynchronous floor request, every time one occurs.
* Timestamp and floor, every time elevator passes a floor.
* Timestamp and floor, every time elevator stops at a floor.<br />
Bonus Enhancement:

Enhance the application as follows: If the elevator has reached its weight limit, it should stop only at floors that were selected from inside the elevator (to let passengers out), until it is no longer at the max weight limit.<br />
Note: For simplicity, the asynchronous request buttons can be entered by the application user via the console, by entering "5U" (request from 5th floor wanting to go Up) or "8D" (request from 8th floor wanting to go Down) or "2" (request from inside elevator wanting to stop at 2nd floor). When the user enters "Q" on the console, the application must end after visiting all floors entered before "Q".

# Execution Instructions:
 - Ensure .Net is installed
 - navigate to Elevator/Elevator
 - execute `dotnet run`
 - begin typing input and hit enter for each request
 - Watch execution output in Elevator/Elevator/ElevatorLog.txt
 - To execute tests, navigate back to ./Elevator/ and run `dotnet test`

# Assumptions:
 - A potential passenger cannot press a button to request an exit until they've actually entered the elevator. A passenger does not have to indicate an exit request.
 - Inputs that do not match the described possible actions are just ignored.
 - Each occupant is allowed to choose 1 exit floor. If the elevator has 3 occupants and 3 exit requests are already pending, no new exit requests will be cached.
 - To both successfully test the durationed nature of the application and complete a test suite quickly, I introduced a `runspeed` parameter for the elevator to be used from the test framework. It allows the elevator to operate 10x faster and is tested thusly.
 - The test framework mocks the logging, due to the parallel nature of the [Test] methods and the single log file. Normally logged output goes to the Console during Test execution, rather than the ElevatorLog.txt file.


# Improvement Ideas:
 - Basement Floors: The array structure I chose to maintain 'inputs' fit better with positive floors, and as the example used positive floor numbers, I stuck with that for this proof of concept. To expand into the basement I could refactor the 'inputs' to a different Collection, or continue using an array, and also maintain an index <-> floor offset across the application, based on abs(lowestFloor).

 - Multiple Cars: This would have truly called for an event stream. An Elevator program with N cars instead of 1 would be a fun and interesting expansion to this challenge.
