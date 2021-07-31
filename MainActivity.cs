using System;
using System.Threading.Tasks;
using Android.App;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace simon2
{
    // [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {

        const int playerSpeed = 600; const int computerSpeed = 800; const int maxLevels = 20; // Defining the constants in the game

        int level = 1; int speed = playerSpeed; int compSpeed = computerSpeed; //800
        int currentPlayersClicks = 0; int score = 0; int points = 10;
        int soundIndex = 0;
        bool flagIsPlayerTurn = false;
        bool flagPlayerdidWrong = false;
        bool flagTryAgain = false; // each "Game On" method I insert a new color to the colors array, only if the flag is false.
                                   // But, if the flag is true, it means the player got a new try of "Game On", and I will not add any new color.
                                   // I will make the flag false again, and will light up the colors that are still in the array.

        ImageButton greenImage; ImageButton redImage; ImageButton yellowImage; ImageButton blueImage; Button startImage; // buttons for the images
        readonly int[] memoryArray = new int[maxLevels];  // this array is the computer memory of what was chosen and the player needs to repeat it
        readonly int[] playerArray = new int[maxLevels];   // this array is the player clicks, and will compare it to memory array
        readonly Random rand = new Random(); // the random will be used in order to light up each time a random imageButton
        readonly MediaPlayer[] soundArray = new MediaPlayer[8];  // will contain all my sounds, in order to avoid sound interrupts between the same color twice,
        // each time I am calling the next sound on the array, that way even if a sound (1.5 seconds) didn't end, there wont be a problem.

        int lifes = 3; TextView theScore;
        TextView theWordLevel; TextView title;                             //TextView theWordScore;

        GridLayout myHeartsLayout;                                         // Hearts layout with it's images
        ImageView leftHeart; ImageView middleHeart; ImageView rightHeart;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RefreshArray(memoryArray);                                     // creating the arrays and initalizing them with 0 values
            RefreshArray(playerArray);                                     // {0, 0, 0, 0, ..., 0}

            base.OnCreate(savedInstanceState);
           
            SetContentView(Resource.Layout.activity_main);

            /*                    *                      *                *                  */
            greenImage = FindViewById<ImageButton>(Resource.Id.greenImg);  // in here I connect between the code objects to the view objects
            redImage = FindViewById<ImageButton>(Resource.Id.redImg);
            yellowImage = FindViewById<ImageButton>(Resource.Id.yellowImg);
            blueImage = FindViewById<ImageButton>(Resource.Id.blueImg);
            startImage = FindViewById<Button>(Resource.Id.buttonStart);   // start button at the beginning of the game            
            /**/
            theWordLevel = FindViewById<TextView>(Resource.Id.levelLabel);
            title = FindViewById<TextView>(Resource.Id.title);

            theScore = FindViewById<TextView>(Resource.Id.scoreLabel);
            myHeartsLayout = FindViewById<GridLayout>(Resource.Id.gridHearts); myHeartsLayout.Visibility = ViewStates.Invisible;
            rightHeart = FindViewById<ImageView>(Resource.Id.heartRight); middleHeart = FindViewById<ImageView>(Resource.Id.heartMiddle);
            leftHeart = FindViewById<ImageView>(Resource.Id.heartLeft);
            /*                               
             *     *                       *                *                
            */
            DisableMyColorButtons();              // disabling the buttons so the user will not be able to click on color buttons, before he starts the game

            InitialSoundArray();                                           // creating all the sounds array and intialize them to the mp3 files in Raw


            greenImage.Click += (e, o) =>
            {
                if (currentPlayersClicks > maxLevels - 1)
                    GameIsDone();                                              //if we reach to MaxLevels moves the game is over
                greenImage.SetImageResource(Resource.Drawable.greenChosen2);   // effect of button being clicked          
                ChangeGreenBack();
                currentPlayersClicks++;                                        // I want to check how many clicks the player has done in each turn               
                if (currentPlayersClicks <= maxLevels)                         // making sure I will not have any out of bounds exception
                    playerArray[currentPlayersClicks - 1] = 1;              
                CheckAfterTurnIfLevel5orPlayerWon();
            };
            yellowImage.Click += (e, o) =>
            {
                if (currentPlayersClicks > maxLevels - 1)
                    GameIsDone();     //if we reach to 20 moves the game is over
                yellowImage.SetImageResource(Resource.Drawable.yellowChosen2);  // effect of button being clicked   
                ChangeYellowBack();
                currentPlayersClicks++;                                        // I want to check how many clicks the player done in each turn             
                if (currentPlayersClicks <= maxLevels)     // making sure I will not have any out of bounds exception
                    playerArray[currentPlayersClicks - 1] = 2;            
                CheckAfterTurnIfLevel5orPlayerWon();
            };
            redImage.Click += (e, o) =>
            {
                if (currentPlayersClicks > maxLevels - 1)
                    GameIsDone();     //if we reach to 20 moves the game is over
                redImage.SetImageResource(Resource.Drawable.redChosen2);    // effect of button being clicked              
                ChangeRedBack();
                currentPlayersClicks++;                                    // I want to check how many clicks the player done in each turn
                if (currentPlayersClicks <= maxLevels)     // making sure I will not have any out of bounds exception
                    playerArray[currentPlayersClicks - 1] = 3;
                CheckAfterTurnIfLevel5orPlayerWon();
            };
            blueImage.Click += (e, o) =>
            {
                if (currentPlayersClicks > maxLevels - 1) GameIsDone();     //if we reach to 20 moves the game is over
                blueImage.SetImageResource(Resource.Drawable.blueChosen2);   // effect of button being clicked   
                ChangeBlueBack();
                currentPlayersClicks++;                                     // I want to check how many clicks the player done in each turn
                if (currentPlayersClicks <= maxLevels)     // making sure I will not have any out of bounds exception
                    playerArray[currentPlayersClicks - 1] = 4;
                CheckAfterTurnIfLevel5orPlayerWon();
            };

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // after starting the game, remove start button, make enable true for color buttons, and add level label
            startImage.Click += delegate (object sender, EventArgs e) {

                DisableMyColorButtons();
                if (lifes == 0)                                              // in case the lifes == 0 (means the player lost all his lifes)
                {                                                            // then I will reDraw all the hearts back
                    DrawAllMyHearts();
                }
                if (lifes == 3)
                {                                                            // if lifes == 3, the game is a new one, so the level is 1
                    theWordLevel.Text = "level: 1";                          // else I dont change it, because it is a try again (so the same level as before)
                    theScore.Text = "score: 0";
                }
                flagIsPlayerTurn = false;
                myHeartsLayout.Visibility = ViewStates.Visible;
                startImage.Visibility = ViewStates.Gone;                     // after start click, erase from screen the start button 
                title.Text = "";                                             // let's play simon!
                GameOn();                                                    //go to gameOn method
            };

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        //making the button click more comftorable to the eye, changing color on click, and after 1 second change back to the same color
        public async void ChangeGreenBack()
        {
            PlayTheSound(); 
           if(flagIsPlayerTurn == false) 
                await Task.Delay(speed);
           else
                await Task.Delay(300);
            greenImage.SetImageResource(Resource.Drawable.green);
        }
        public async void ChangeRedBack()
        {
            PlayTheSound();
            if (flagIsPlayerTurn == false)
                await Task.Delay(speed);
            else
                await Task.Delay(300);
            redImage.SetImageResource(Resource.Drawable.red);
        }
        public async void ChangeYellowBack()
        {
            PlayTheSound();
            if (flagIsPlayerTurn == false)
                await Task.Delay(speed);
            else
                await Task.Delay(300);
            yellowImage.SetImageResource(Resource.Drawable.yellow);
        }
        public async void ChangeBlueBack()
        {
            PlayTheSound();
            if (flagIsPlayerTurn == false)
                await Task.Delay(speed);
            else
                await Task.Delay(300);
            blueImage.SetImageResource(Resource.Drawable.blue);
        }
        /*                         *                             *                      *                */
        public void GameOn()
        {
            DisableMyColorButtons();
            if (level > memoryArray.Length)
                YouWon();
            else
            {
                int randNum = rand.Next(1, 5);  //  rand 1-4 (each color has a number)
                if (!flagTryAgain)
                {
                    PushRandToArray(randNum);       //  insert to memoryArray
                }
                else
                {
                    flagTryAgain = false;
                }
                TurnColors(level);              //  each level there will be more colors turned on
                PlayerTurn();                   //  after the computer had randomly turn on buttons, it is the player turn!       
            }
        }

        public void PushRandToArray(int rand)
        {
            int index = ReturnTheNextIndex(memoryArray);  //here rand will be added to the memoryArray
            memoryArray[index] = rand;
        }

        public async void TurnColors(int clicks)  // 1 -> green turn on  2 -> yellow turn on  3 -> red turn on  4 > blue turn on 
        {
            DisableMyColorButtons();
            await Task.Delay(2000);
            for (int i = 0; i < clicks; i++)            // the bututons will be pressed Clicks times.
            {
                switch (memoryArray[i])                  // from the pushRandArray to the computer array
                {
                    case 1:
                        TurnOnGreen();
                        break;
                    case 2:
                        TurnOnYellow();
                        break;
                    case 3:
                        TurnOnRed();
                        break;
                    case 4:
                        TurnOnBlue();
                        break;
                }
                await Task.Delay(compSpeed);
            }
            flagPlayerdidWrong = false;
            if (!flagPlayerdidWrong)
                EnableMyColorButtons();                    // after the clicks happend, only then the player can start his turn.
        }

        public async void PlayerTurn()
        {
            if (!flagPlayerdidWrong)
            {
                EnableMyColorButtons();
                flagIsPlayerTurn = true;
            }
            if (startImage.Text == "Can You Do Better?")        // every 5 levels this will pop up and after 2.5 seconds it will be removed
            {
                await Task.Delay(2500);
                startImage.Visibility = ViewStates.Invisible;
                startImage.Enabled = true;
            }
        }

        /*                         *                               *                           *                                                    */
        public bool CheckIfPlayerCorrect()                 // comparing the two arrays
        {
            for (int i = 0; i < currentPlayersClicks; i++)
            {
                if (memoryArray[i] != playerArray[i])
                {
                    DisableMyColorButtons();
                    flagPlayerdidWrong = true;
                    return false;
                }
            }
            return true;
        }
        /*                         *                               *                           *                                                    */
        public int[] RefreshArray(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0;
            }
            return arr;
        }

        public void InitialSoundArray()  // intializng the sounds, the even places will have sound A, the odd ones will have sound B
        {
            for (int i = 0; i < 8; i += 2)
            {
                soundArray[i] = MediaPlayer.Create(this, Resource.Raw.greenSound);
            }
            for (int i = 1; i < 8; i += 2)
            {
                soundArray[i] = MediaPlayer.Create(this, Resource.Raw.blueS);
            }
        }

        public void PlayTheSound()
        {
            soundIndex++;                          // to make sure I will not try to use the same sound twice, before it ended playing 
            if (soundIndex == 800)
                soundIndex = 0;                    // ! soundIndex is an Integr, making sure it will not go higher for no reason, to avoid size exception
            soundArray[soundIndex % 8].Start();      // 8 places in the array, 
        }

        /*                        *                     *                      *                    *                     *                        */
        public void TurnOnGreen()
        {
            greenImage.SetImageResource(Resource.Drawable.greenChosen2);
            ChangeGreenBack();
        }
        public void TurnOnYellow()
        {
            yellowImage.SetImageResource(Resource.Drawable.yellowChosen2);
            ChangeYellowBack();
        }
        public void TurnOnRed()
        {
            redImage.SetImageResource(Resource.Drawable.redChosen2);
            ChangeRedBack();
        }
        public void TurnOnBlue()
        {
            blueImage.SetImageResource(Resource.Drawable.blueChosen2);
            ChangeBlueBack();
        }
        /*                           *                              *                          *                            *                              */
        public int ReturnTheNextIndex(int[] arr)
        {
            int index = 0;
            for (int i = 0; i < maxLevels; i++)          // letting the program to know where it can insert the next number to each array
            {
                if (arr[i] == 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /*                  *                      *                        *                       *                         *                          */
        public void EnableMyColorButtons()
        {
            if (!flagPlayerdidWrong)
            {
                flagIsPlayerTurn = true;
                greenImage.Enabled = true;
                redImage.Enabled = true;
                yellowImage.Enabled = true;
                blueImage.Enabled = true;
            }
        }
        public void DisableMyColorButtons()
        {
            flagIsPlayerTurn = false;
            greenImage.Enabled = false;
            redImage.Enabled = false;
            yellowImage.Enabled = false;
            blueImage.Enabled = false;
        }

        public void GameIsDone()
        {
            DisableMyColorButtons();
            lifes--;
            switch (lifes)
            {
                case 2:
                    rightHeart.SetImageResource(Resource.Drawable.heartBlack);
                    SwitchGameIsDone();
                    break;
                case 1:
                    leftHeart.SetImageResource(Resource.Drawable.heartBlack);
                    SwitchGameIsDone();
                    break;
                default:                                                             // in case the player has 0 lifes, I restet his score, both of the arrays
                    middleHeart.SetImageResource(Resource.Drawable.heartBlack);     // , player array and computer, level will go back to 0.
                    DisableMyColorButtons();
                    ResetEverythingForNewTry();
                    flagTryAgain = false;
                    theScore.Text = "score: 0";
                    theWordLevel.Text = "";
                    break;
            }
            startImage.Text = "Try Again?";
            DisableMyColorButtons();
            startImage.Visibility = ViewStates.Visible;

        }
        public void ResetEverythingForNewTry()  // Reseting all values to the new game values.
        {
            DisableMyColorButtons();
            level = 1;
            currentPlayersClicks = 0;
            score = 0;
            points = 10;
            speed = playerSpeed;
            compSpeed = computerSpeed;
            RefreshArray(memoryArray);
            RefreshArray(playerArray);
            theWordLevel.Text = "";
        }

        public void CheckAfterTurnIfLevel5orPlayerWon()
        {
            if (currentPlayersClicks == level)
            {
                if (CheckIfPlayerCorrect())                                // sending to check if there is a a difference between the two arrays
                {
                    if (level % 5 == 0)                                    // every 5 levels the game becomes harder (less time to remember)
                    {                                                      // lowering the delay of computer speed
                        GetFaster();
                    }

                    if (level == maxLevels)
                        YouWon();
                    else
                    {
                        NextTurnReset();                                   // clearing player array, and intialize his clicks to 0
                    }
                }
                else
                {
                    GameIsDone();
                }
            }
            else
            {
                if (!CheckIfPlayerCorrect())
                    GameIsDone();
            }
        }

        public void YouWon()
        {
            DisableMyColorButtons();
            DrawAllMyHearts();          // in case a player won, he will get all hearts back, and a new game will begin
            currentPlayersClicks = 0;
            level = 1;
            startImage.Visibility = ViewStates.Visible;
            startImage.Text = "Start";
            startImage.Enabled = true;
            RefreshArray(memoryArray);
            ResetEverythingForNewTry();
        }
        /*                    *                     *                        *                      *                              *                          */

        public async void GetFaster() //leveling up in case the player got to a %5==0 level' making the game faster!
        {
            compSpeed -= 60;
            speed -= 40;
            points += 120;
            RefreshArray(playerArray);
            theWordLevel.Text = "Amazing!";
            startImage.Visibility = ViewStates.Visible;
            startImage.Text = "Can You Do Better?";
            startImage.Enabled = false;
            await Task.Delay(1000);
        }

        public void NextTurnReset() // after each turn the labels and vars need an update
        {
            level++;
            theWordLevel.Text = "level: " + (level).ToString();
            score += points;
            theScore.Text ="score: "+(score).ToString();
            currentPlayersClicks = 0;                               // new turn , new set of clicks                     
            GameOn();
            DisableMyColorButtons();
        }

        public void DrawAllMyHearts()
        {
            DisableMyColorButtons();
            rightHeart.SetImageResource(Resource.Drawable.heart2);  //reDrawing the hearts
            leftHeart.SetImageResource(Resource.Drawable.heart2);
            middleHeart.SetImageResource(Resource.Drawable.heart2);
            lifes = 3;                                              // if the program arrived to here, then the player lost all his hearts
        }

        public void SwitchGameIsDone()
        {
            RefreshArray(playerArray);               // in case the player has two more lifes, I clear his array (and errors)
            DisableMyColorButtons();
            currentPlayersClicks = 0;               // clicks are intialized to 0
            flagTryAgain = true;                    // and he will get another try
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
