using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
	public class UIManager : MonoBehaviour
	{
		[Serializable]
		public class multiplayerGameLobby
		{
			public UIPlaceholder WeaponPlaceholder;
			public UIPlaceholder GameModePlaceholder;
			public UIPlaceholder MapPlaceholder;
			public UIPlaceholder AvatarPlaceholder;
			
			public UIHelper.LobbyMainUI MainMenu;
			public UIHelper.LobbyGameModesUI GameModesMenu;
			public UIHelper.LobbyMapsUI MapsMenu;
			public UIHelper.LobbyLoadoutUI LoadoutMenu;
			public UIHelper.AvatarsMenu AvatarsMenu;
			public UIHelper.LobbyCharactersMenu CharactersMenu;
		}

		[Serializable]
		public class multiplayerGameRoom
		{
			public UIPlaceholder PlayerInfoPrefab;
			public UIPlaceholder MatchStatsPrefab;
			public PUNHelper.GameOverMenu GameOverMenu;
			public PUNHelper.SpectateMenu SpectateMenu;
			public PUNHelper.MatchStats MatchStats;
			public PUNHelper.PauseMenu PauseMenu;
			public PUNHelper.StartMenu StartMenu;
			public PUNHelper.TimerBeforeMatch TimerBeforeMatch;
			public PUNHelper.TimerAfterDeath TimerAfterDeath;
			public PUNHelper.PreMatchMenu PreMatchMenu;
		}

		public multiplayerGameLobby MultiplayerGameLobby;
		public multiplayerGameRoom MultiplayerGameRoom;
		public UIHelper.CharacterUI CharacterUI;
		public UIHelper.singlePlayerGame SinglePlayerGame;
		
		public Button[] uiButtons = new Button[17];
		public bool[] buttonsWereActive = new bool [15];
		
		public GameObject UIButtonsMainObject;
		public GameObject moveStick;
		public GameObject moveStickOutline;
		public GameObject cameraStick;
		public GameObject cameraStickOutline;
		
		#region InspectorVariables
		
		public int inspectorTab;

		public int multiplayerGameInspectorTab;
		
		public int roomInspectorTabTop;
		public int roomInspectorTabDown;
		public int currentRoomInspectorTab;
		public int roomMatchStatsTab;

		public int lobbyInspectorTabTop;
		public int lobbyInspectorTabDown;
		public int currentLobbyInspectorTab;
		
		public int characterUiInspectorTab;
		public int curWeaponSlot;

		#endregion

		private void Awake()
		{
			if(MultiplayerGameLobby.WeaponPlaceholder)
				MultiplayerGameLobby.WeaponPlaceholder.gameObject.SetActive(false);
			
			if(MultiplayerGameLobby.MapPlaceholder)
				MultiplayerGameLobby.MapPlaceholder.gameObject.SetActive(false);
			
			if(MultiplayerGameLobby.GameModePlaceholder)
				MultiplayerGameLobby.GameModePlaceholder.gameObject.SetActive(false);
			
			if(MultiplayerGameLobby.AvatarPlaceholder)
				MultiplayerGameLobby.AvatarPlaceholder.gameObject.SetActive(false);

			if(MultiplayerGameRoom.MatchStatsPrefab)
				MultiplayerGameRoom.MatchStatsPrefab.gameObject.SetActive(false);
			
			if(MultiplayerGameRoom.PlayerInfoPrefab)
				MultiplayerGameRoom.PlayerInfoPrefab.gameObject.SetActive(false);
			
//			if(MultiplayerGameRoom.PlayerIconPrefab)
//				MultiplayerGameRoom.PlayerIconPrefab.gameObject.SetActive(false);
		}

		public void HideAllMultiplayerLobbyUI()
		{
			MultiplayerGameLobby.MainMenu.DisableAll();
			MultiplayerGameLobby.LoadoutMenu.DisableAll();
			MultiplayerGameLobby.MapsMenu.DisableAll();
			MultiplayerGameLobby.GameModesMenu.DisableAll();
			MultiplayerGameLobby.AvatarsMenu.DisableAll();
			MultiplayerGameLobby.CharactersMenu.DisableAll();
		}
		
		public void HideAllMultiplayerRoomUI()
		{
			MultiplayerGameRoom.SpectateMenu.DisableAll();
            
			MultiplayerGameRoom.MatchStats.DisableAll();

			MultiplayerGameRoom.GameOverMenu.DisableAll();
            
			MultiplayerGameRoom.PauseMenu.DisableAll();
            
			MultiplayerGameRoom.StartMenu.DisableAll();
            
			MultiplayerGameRoom.TimerBeforeMatch.DisableAll();
            
			MultiplayerGameRoom.TimerAfterDeath.DisableAll();
           
			MultiplayerGameRoom.PreMatchMenu.DisableAll();
           
			if(MultiplayerGameRoom.MatchStats.KillStatsContent)
				MultiplayerGameRoom.MatchStats.KillStatsContent.gameObject.SetActive(false);
		}
	}
}
