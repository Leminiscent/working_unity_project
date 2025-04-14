<a id="readme-top"></a>

<div align="center">
<h1 align="center">Prototype Portfolio RPG</h1>
<h4>Designed and developed by: Lemuel Nogueira</h4>

[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=plastic&logo=unity)](https://unity3d.com)
</div>

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About the Project</a></li>
    <li><a href="#features">Features</a></li>
    <li><a href="#how-to-play">How to Play</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

## About The Project

This project is a demonstration of a 2D monster-taming role-playing game developed in Unity. The game assigns the role of Commander to the player to control a group of battlers. Gameplay includes world exploration, combat, and party management. The game world contains rival Commanders and non-player characters. The battle system supports multiple actions and includes a process to recruit opposing battlers under specified conditions.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Features

- **Main Menu:**  

  Provides options to resume saved progress, start a new session, or exit the game.  
  
  ![Main Menu](Media/Images/main_menu.png)

- **Character Selection:**  

  Provides a selection screen listing characters with assigned types and statistics. The chosen character represents the Commander.  

  ![Character Selection Screen](Media/Images/character_select_screen.png)  

  ![Game Start](Media/GIFs/game_start.gif)

- **World Exploration and Interaction:**  

  Allows navigation of the game world and interaction with non-player characters, merchants, innkeepers, and quest givers.  

  ![Overworld](Media/Images/gameplay_village.png)  

  ![Item Obtained](Media/Images/gameplay_item_obtained.png)  

  ![Gameplay](Media/GIFs/gameplay.gif)  

  ![Shopping](Media/GIFs/shopping.gif)

- **Party Management:**  

  Displays the party configuration with a maximum of six battlers (one Commander and up to five additional battlers). Provides functionality to reorder battlers and view summaries.  

  ![Party Screen](Media/Images/party_screen.png)  

  ![Summary Screen](Media/Images/summary_screen.png)

- **Deputy Mechanics:**  

  Designates the second battler (or the first in certain configurations) as deputy, who follows the Commander.

- **Inventory System:**  

  Organizes items by category, displays the current GP (gold pieces), and allows item usage from the menu.

- **Barracks Organization:**  

  Manages recruited battlers using depots with 48 slots each. Permits movement of battlers between the party and storage while preserving the Commander’s position.  

  ![Barracks](Media/Images/barracks.png)

- **Battle System:**  

  Implements a turn-based combat system with six actions:

  - **Fight:** Select moves that consume SP; if SP is insufficient, a fallback move is executed.
  - **Talk:** Initiates a recruitment process in which the target battler questions the player.
  - **Item:** Opens the inventory for item usage.
  - **Guard:** Lowers damage received during the round.
  - **Switch:** Permits switching battlers during combat.
  - **Run:** Attempts to exit the battle.
  
  ![Commander Battle](Media/Images/gameplay_battle_3.png)  

  ![Move Target Selection](Media/Images/gameplay_battle_target_selection.png)  

  ![Rogue Battle](Media/GIFs/battle.gif)

- **Battle Variants:**  

  Differentiates between Commander and Rogue battles. Rogue battles include a recruitment mechanism through dialogue.  

  ![Recruitment](Media/Images/gameplay_battle_recruitment.png)

- **Experience and Leveling:**  

  Awards XP for defeated enemies and supports leveling up, move acquisition, and transformations under specific conditions.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## How to Play

1. **Launch the Game:**  
   - Access the main menu. Options include resuming saved progress, starting a new session, or exiting.

2. **Character Selection:**  
   - Select a Commander from the list of characters with assigned types and statistics.

3. **Exploration:**  
   - Navigate the game world and interact with non-player characters for services, quests, and information.
   - Open the in-game menu outside battles or cutscenes to access the party, inventory, and barracks.

4. **Party and Inventory Management:**  
   - Use the party screen to view battler details and reorder the lineup.
   - Access the inventory to view items by category and the current GP balance.
   - Use the barracks screen to manage additional battlers; navigate among depots and slots using keyboard inputs.

5. **Battle Operations:**  
   - Initiate battles by encountering enemy Commanders or rogue battlers.
   - Select battle actions from the following options: Fight, Talk, Item, Guard, Switch, and Run.
   - During the Talk action in rogue battles, answer the questions posed by opposing battlers.
   - When using the Fight action, manage move selection relative to SP consumption; execute a fallback move if SP is insufficient.
   - Enemy battlers act randomly until advanced AI is implemented.
   - Victories award XP, possible item drops, and may trigger transformations for eligible battlers.

6. **Game Over and Save Operations:**  
   - After all player battlers are defeated, return to the main menu.
   - Use save options available within the in-game menu during exploration.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Roadmap

- [ ] Relocate the battler storage screen from the game menu to a dedicated barracks facility.
- [ ] Integrate commander and deputy abilities.
- [ ] Implement a crafting system for creating items from battle loot.
- [ ] Develop sorting methods for the barracks and inventory.
- [ ] Improve dialogue visualization using camera offset/zoom, overhead icons, and portraits.
- [ ] Extend recruitment mechanics with additional dialogue options.
- [ ] Implement enemy AI that evaluates battle state and battler personality profiles.
- [ ] Expand the game world with more areas and interactive elements.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contact

Email: [nogueiralemuel@gmail.com](mailto:nogueiralemuel@gmail.com)  
LinkedIn: [https://www.linkedin.com/in/lemuel-nogueira/](https://www.linkedin.com/in/lemuel-nogueira/)

<p align="right">(<a href="#readme-top">back to top</a>)</p>
