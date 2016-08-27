# Star Wars: Imperial Chess/ Endor Battlefront

A standalone Star Wars fan-made game for the Windows platform.

Relive the Battle of Endor with a friend in this cinematics-driven 3D, two-player chess game.

Built using the Unity Engine.

Uses the original Star Wars soundtrack, composed by John Williams.

All 3D models built by Abhishek Arora using Blender:

Piece: <White, Black>

  Pawn: X-Wing, TIE Fighter
  
  Bishop: Y-Wing, TIE Bomber
  
  Knight: Millennium Falcon, Slave 1
  
  Rook: Alderaan Cruiser, Star Destroyer
  
  Queen: Princess Leia Organa, Darth Vader
  
  King: Luke Skywalker, Darth Sidious
  

Code structure:

[Files/classes come under 3 categories:

Chess Engine classes: Piece, board and movement representations that are oblivious to Unity constructs.

Behaviour Classes: Classes that are hooked into Unity's event framework and attached to GameObjects.

Helper Classes: Instantiable/static classes that are used by the above types of classes and by each other.

]

<Assets/Scripts/>

  Bitboard.cs : The Chess Engine class that defines the Bitboard, a representation of a chess board state that allows for fast computations
  
  Chessboard.cs : The Chess Engine class that represents a Chessboard/Board State
  
  Constants.cs : The helper class that hosts all Constants used throughout the project
  
  EventManager.cs : The Behaviour script that controls all non-Unity events triggered through the project
  
  Gamekeeper.cs (Entry point) : The Behaviour script that controls the game flow, right from the opening crawl to the end of the game
  
  Movement.cs : The helper file which defines Movement-related classes for use throughout the game and handles movement (and only triggers animations) of piece GameObjects
  
  <Animation/>
  
    ActionCamera.cs : The Behaviour script that controls the auxiliary camera and audio used to dramatize piece captures/deaths
    
    Animations.cs : The helper class for all animations
    
    OpeningCrawl.cs : The Behaviour script that controls the Opening Crawl and triggers the rotation of the camera towards the board
    
    StarWarsLogo.cs : The Behaviour script that triggers the opening crawl
    
    TurnIcon.cs : The Behaviour script that controls the faction logo that shows up before each turn to indicate the start of a turn
    
  <Pieces/>
  
    Bishop.cs : The Chess Engine class that controls Bishops
    
    King.cs : The Chess Engine class that controls Kings
    
    Knight.cs : The Chess Engine class that controls Knights
    
    MoveIcon.cs : The Behaviour script associated with the visible icons on the board that the player can click on to execute a valid move
    
    Pawn.cs : The Chess Engine class that controls Pawns
    
    Piece.cs : The Chess Engine Abstract class for all pieces that defines some of their functionality, but leaves some to be implemented by them
    
    PieceBehaviour.cs : The Behaviour script attached to Piece game objects which handles their Unity events and hosts a Chess Engine piece for engine computations
    
    Queen.cs : The Chess Engine class that controls Queens
    
    Rook.cs : The Chess Engine class that controls Rooks
