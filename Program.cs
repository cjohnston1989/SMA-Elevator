using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElevatorChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            Elevator elevator = new Elevator();
            while(elevator.inService){
                string input = Console.ReadLine();
                elevator.handleInput(input);
                // Console.WriteLine(input);
            }
            Task.WaitAll(elevator.taskArray.ToArray());
        }
    }

    /// <summary>
    /// This class encapsulates primary responsibilities of the Elevator.
    /// It has an input queue
    /// It has some state
    /// </summary>
    class Elevator
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
        public (int,int)[] inputs;
        public Logger logger;
        public List<Task> taskArray = new List<Task>();
        private System.ComponentModel.BackgroundWorker carWorker;
        // flat numbers here can be from a config file? starting small for ease of testing
        // each weight is 1 person. isn't it nice we all weigh the same?
        public Elevator(int floors = 5, int maxWeight = 3){
            this.logger = new Logger();
            this.floors = floors;
            this.maxWeight = maxWeight;
            this.direction = "U";
            this.currentFloor = 1;
            this.nextFloor = 1;
            this.state = 0;
            this.weight = 0;
            this.requests = 0;
            this.inputs = new (int,int)[floors];
            inService = true;
            // InitializeBackgroundWorker();
        }

        public void handleInput(string input){
            // Do some input validation ( should be Q or an integer greater than 0 and less than or equal to maxfloor or such a number with either a U or a D after it)
            if(input is not null) {
                logger.log("pressed "+input);
                if(input == "Q"){
                    stopElevator();
                }
                // Modify the inputs array
                int exitFloor = 0;
                bool isNumber = int.TryParse(input, out exitFloor);
                // Someone wants to exit at a valid floor
                if(isNumber){
                    if(exitFloor > 0 && exitFloor <= floors){
                        inputs[exitFloor-1] = (inputs[exitFloor-1].Item1,inputs[exitFloor-1].Item2+1);
                        // This will handle some bullet 4 edge cases
                        if(     (exitFloor > (currentFloor + state) && direction == "U") ||
                                (exitFloor < (currentFloor - state) && direction == "D")) {
                            nextFloor = exitFloor;
                        }
                        this.requests++;
                    }
                }
                else {
                    string entranceDirection = input.Substring(input.Length-1).ToUpper();
                    string entranceFloorString = input.Substring(0,input.Length-1);
                    int entranceFloor = 0;
                    bool entranceIsNumber = int.TryParse(entranceFloorString, out entranceFloor);
                    // Someone on a valid floor wants to enter and has a valid direction
                    if(entranceIsNumber && (entranceFloor > 0) && (entranceFloor <= floors) && (entranceDirection=="D" || entranceDirection=="U")){
                        inputs[entranceFloor-1] = (inputs[entranceFloor-1].Item1 + 1,inputs[exitFloor-1].Item2);
                        if(     ((exitFloor > (currentFloor + state) && direction == "U") ||
                                (exitFloor < (currentFloor - state) && direction == "D")) &&
                                maxWeight > weight) {
                            nextFloor = exitFloor;
                        }
                        this.requests++;
                    }
                }

                // Make sure the elevator is moving
                this.taskArray.Add(Task.Run(() => moveElevator(inputs)));
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
                if(direction == "U" && currentFloor == floors){
                    this.direction = "D";
                    this.nextFloor = findNextFloor();
                }
                if(direction == "D" && currentFloor == 1){
                    this.direction = "U";
                    this.nextFloor = findNextFloor();
                }
                if(nextFloor == currentFloor){
                    this.nextFloor = findNextFloor();
                }

                // We have a target and no more work to do on this floor, lets move
                this.state = 1;
                await Task.Delay(3000);
                this.currentFloor = this.direction == "U" ? this.currentFloor+1 : this.currentFloor-1;

                if(currentFloor == nextFloor){
                    this.logger.log("stopped at "+currentFloor.ToString());
                    await Task.Delay(1000);
                    // Doors are opening
                    this.weight -= inputs[currentFloor-1].Item2;
                    this.requests -= inputs[currentFloor-1].Item2;
                    inputs[currentFloor-1] = (inputs[currentFloor-1].Item1, 0);
                    while(this.weight < this.maxWeight && inputs[currentFloor-1].Item1 > 0){
                        this.weight += 1;
                        this.requests -= 1;
                        inputs[currentFloor-1] = (inputs[currentFloor-1].Item1 - 1, inputs[currentFloor-1].Item2);
                    }
                }
                else{
                    this.logger.log("passing "+currentFloor.ToString());
                }
            }
        }

        private int findNextFloor(){
            if(this.direction == "U") {
                for(int i = currentFloor + 1; i < floors; i++){
                    if((this.maxWeight > this.weight && inputs[i-1].Item1 > 0) || inputs[i-1].Item2 > 0){
                        return i;
                    }
                }
            }
            else {
                for(int i = currentFloor - 1; i > 0; i--){
                    if((this.maxWeight > this.weight && inputs[i-1].Item1 > 0) || inputs[i-1].Item2 > 0){
                        return i;
                    }
                }
            }
            return currentFloor;
        }

    }

    
    /// <summary>
    /// This is responsible for all of the log output for user inputs and floor interactions
    /// </summary>
    class Logger
    {
        public Logger(){
            File.Create("ElevatorLog.txt").Close();
        }
        public async void log(string data){
            using StreamWriter file = new("ElevatorLog.txt", append: true);
            await file.WriteLineAsync(""+DateTime.Now + "    " + data);
        }
    }
}
