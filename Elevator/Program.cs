using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ElevatorChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            Elevator elevator = new Elevator(5, 3);
            while(elevator.inService){
                string input = Console.ReadLine();
                elevator.handleInput(input);
            }
            Task.WaitAll(elevator.taskArray.ToArray());
        }
    }

    /// <summary>
    /// This class encapsulates primary responsibilities of the Elevator.
    /// It has an inputs structure that tracks requests to get on or off per floor
    /// It has some state
    /// </summary>
    public class Elevator
    {
        private int floors;
        private int maxWeight;

        /// Valid values: ["U","D"]
        private string direction;
        private int currentFloor;
        private int nextFloor;

        /// Valid values:
        ///   0 - Stopped
        ///   1 - Moving
        private int state;
        private int weight;
        public bool inService;
        private int requests;
        private int exitRequests;
        public (int,int)[] inputs;
        private int runspeed;
        public Logger logger;
        public List<Task> taskArray = new List<Task>();
        // flat numbers here can be from a config file? starting small for ease of testing
        // each weight is 1 person. isn't it nice we all weigh the same?
        public Elevator(int floors = 5, int maxWeight = 3, bool mockLog = false, int runspeed = 1){
            this.logger = new Logger(mockLog);
            this.runspeed = runspeed;
            this.floors = floors;
            this.maxWeight = maxWeight;
            this.direction = "U";
            this.currentFloor = 1;
            this.nextFloor = 1;
            this.state = 0;
            this.weight = 0;
            this.requests = 0;
            this.exitRequests = 0;
            this.inputs = new (int,int)[floors];
            inService = true;
        }

        public async void handleInput(string input){
            // Do some input validation ( should be Q or an integer greater than 0 and less than or equal to maxfloor or such a number with either a U or a D after it)
            if(input is not null) {
                logger.log("pressed "+input + "    "+inpstr());
                if(input == "Q"){
                    stopElevator();
                    this.taskArray.Add(Task.Run(() => moveElevator(inputs)));
                    return;
                }
                // Modify the inputs array
                int exitFloor;
                bool isNumber = int.TryParse(input, out exitFloor);
                // Someone wants to exit at a valid floor
                if(isNumber && weight > exitRequests){
                    if(exitFloor > 0 && exitFloor <= floors){
                        inputs[exitFloor-1] = (inputs[exitFloor-1].Item1,inputs[exitFloor-1].Item2+1);
                        // This might not be appropriate here /This will handle some bullet 4 edge cases/
                        // if(     (exitFloor > (currentFloor + state) && direction == "U") ||
                        //         (exitFloor < (currentFloor - state) && direction == "D")) {
                        //     nextFloor = exitFloor;
                        // }
                        this.requests++;
                        this.exitRequests++;
                    }
                }
                else if (!isNumber) {
                    string entranceDirection = input.Substring(input.Length-1).ToUpper();
                    string entranceFloorString = input.Substring(0,input.Length-1);
                    int entranceFloor;
                    bool entranceIsNumber = int.TryParse(entranceFloorString, out entranceFloor);
                    // Someone on a valid floor wants to enter and has a valid direction
                    if(entranceIsNumber && (entranceFloor > 0) && (entranceFloor <= floors) && (entranceDirection=="D" || entranceDirection=="U")){
                        inputs[entranceFloor-1] = (inputs[entranceFloor-1].Item1 + 1,inputs[entranceFloor-1].Item2);
                        // same as above, maybe don't modify nextfloor logic from the handleinput section
                        // if(     ((entranceFloor > (currentFloor + state) && direction == "U") ||
                        //         (entranceFloor < (currentFloor - state) && direction == "D")) &&
                        //         maxWeight > weight) {

                        //     nextFloor = entranceFloor;
                        // }
                        this.requests++;
                    }
                }

                // Make sure the elevator is moving
                Console.WriteLine("R"+this.requests);
                if(this.requests == 1 || (this.state == 0 && this.weight == this.maxWeight)){await Task.Run(() => moveElevator(inputs));}
                // Task.WaitAll(this.taskArray.ToArray());
                // this.taskArray.Add(Task.Run(() => moveElevator(inputs)));
            }
        }

        public void stopElevator(){
            inService = false;
        }


        private async void moveElevator((int,int)[] inputs){
            // Make the elevator move
            // At each loop entry we should have finished arrival at a floor and related tasks
            while(this.requests > 0){
                // We're sitting at the top
                if(currentFloor == floors){
                    this.direction = "D";
                    this.nextFloor = findNextFloor();
                }
                else if(currentFloor == 1){
                    this.direction = "U";
                    this.nextFloor = findNextFloor();
                }
                if(nextFloor == currentFloor){
                    this.nextFloor = findNextFloor();
                }
                if(nextFloor < currentFloor){
                    this.direction = "D";
                    this.nextFloor = findNextFloor();
                }
                else if(nextFloor > currentFloor){
                    this.direction = "U";
                    this.nextFloor = findNextFloor();
                }
                else{
                    this.logger.log("no more requests, pausing");
                    return;
                }

                // We have a target and no more work to do on this floor, lets move
                this.state = 1;
                await Task.Delay(3000 / runspeed);
                this.currentFloor = this.direction == "U" ? this.currentFloor+1 : this.currentFloor-1;


                if(currentFloor == nextFloor){
                    this.state=0;
                    this.logger.log("stopped at "+currentFloor.ToString() + "    "+inpstr());
                    await Task.Delay(1000 / runspeed);
                    // Doors are opening
                    this.weight -= inputs[currentFloor-1].Item2;
                    this.requests -= inputs[currentFloor-1].Item2;
                    this.exitRequests -= inputs[currentFloor-1].Item2;
                    inputs[currentFloor-1] = (inputs[currentFloor-1].Item1, 0);
                    while(this.weight < this.maxWeight && inputs[currentFloor-1].Item1 > 0){
                        this.weight += 1;
                        this.requests -= 1;
                        inputs[currentFloor-1] = (inputs[currentFloor-1].Item1 - 1, inputs[currentFloor-1].Item2);
                    }
                }
                else{
                    this.logger.log("passing "+currentFloor.ToString() + "    "+inpstr());
                }
            }
            this.logger.log("no more requests, pausing");
        }

        private int findNextFloor(){
            if(this.direction == "U") {
                for(int i = currentFloor + this.state; i <= floors; i++){
                    if((this.maxWeight > this.weight && inputs[i-1].Item1 > 0) || inputs[i-1].Item2 > 0){
                        logger.log("nextFloor determined to be "+i);
                        return i;
                    }
                }
                this.direction = "D";
                for(int i = currentFloor; i > 0; i--){
                    if((this.maxWeight > this.weight && inputs[i-1].Item1 > 0) || inputs[i-1].Item2 > 0){
                        logger.log("nextFloor determined to be "+i+" and turning around");
                        return i;
                    }
                }
            }
            else {
                for(int i = currentFloor - this.state; i > 0; i--){
                    if((this.maxWeight > this.weight && inputs[i-1].Item1 > 0) || inputs[i-1].Item2 > 0){
                        logger.log("nextFloor determined to be "+i);
                        return i;
                    }
                }
                this.direction = "U";
                for(int i = currentFloor; i <= floors; i++){
                    if((this.maxWeight > this.weight && inputs[i-1].Item1 > 0) || inputs[i-1].Item2 > 0){
                        logger.log("nextFloor determined to be "+i+" and turning around");
                        return i;
                    }
                }
            }
            logger.log("nextFloor defaulted to "+currentFloor);
            return currentFloor;
        }

        private string inpstr(){
            string os = " "+this.weight + "|"+this.currentFloor+"|"+this.nextFloor+"    ";
            foreach((int,int) i in inputs){
                os = os + " "+i.Item1 + "|" + i.Item2 + " ";
            }

            return os;
        }

        public int getCurrentFloor(){
            Console.WriteLine("F"+this.currentFloor);
            return this.currentFloor;
        }

        public int getWeight(){
            Console.WriteLine("W"+this.weight);
            return this.weight;
        }

    }

    
    /// <summary>
    /// This is responsible for all of the log output for user inputs and floor interactions
    /// </summary>
    public class Logger
    {
        private bool mock;
        public Logger(bool mock = false){
            this.mock = mock;
            if(!mock){
                File.Create("ElevatorLog.txt").Close();
            }
        }
        public async void log(string data){
            if(this.mock){
                Console.WriteLine(data);
                return;
            }
            using StreamWriter file = new("ElevatorLog.txt", append: true);
            await file.WriteLineAsync(""+DateTime.Now + "    " + data);
        }
    }
}
