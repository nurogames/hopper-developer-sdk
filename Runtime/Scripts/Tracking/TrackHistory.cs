using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using VRWeb.Events;
using VRWeb.Managers;
using VRWeb.Rig;
using VRWeb.User;
using VRWeb.Utils;
using VRWeb.VRML;
using VRWeb.VRML.Protocols;

namespace VRWeb.Tracking
{

	public class TrackHistory : HopperManagerMonoBehaviour < TrackHistory >, ITrackHistory
    {
        public string FilePath => Path.Join( Application.persistentDataPath, "history.json" );

        public string FavoritesFilePath => Path.Join( Application.persistentDataPath, "favorites.json" );

        public bool IsTracklistEmpty => TrackList.Count == 0;

        public bool IsLoadingTrackFile { get; private set; }

        public bool IsActiveTracking { get; set; }

        public List < TrackHistoryEntry > TrackList => m_TrackList.History;

        public List < TrackHistoryEntry > Favorites => m_Favorites.History;

        private TrackList m_TrackList;
        private TrackList m_Favorites;

        void Awake()
        {
            IsActiveTracking = false;
            RegisterManager();
        }

        void Start()
        {
            m_TrackList = new();
            m_TrackList.History = new();

            m_Favorites = new();
            m_Favorites.History = new();

            if ( !File.Exists( FilePath ) || !File.Exists( FavoritesFilePath ) )
            {
                Debug.Log("No history file found, creating new one");
                Save();
            }

            Load();
        }

        private void OnDestroy()
        {
            Save();
        }

		private void Update()
		{
            Track();
		}

		public void OnInvokeSceneLoaded( SceneLoadedEvent.LoadStatus loadStatus )
        {
            if ( loadStatus == SceneLoadedEvent.LoadStatus.finishLoad )
                StartCoroutine( StartTracking() );
        }

        private IEnumerator StartTracking()
        {
            yield return new WaitForSeconds( 1 );

            IsActiveTracking = true;
        }

		private void Track()
		{
            if (!IsActiveTracking)
                return;

            if (UserRig.Instance == null)
                return;
            
			var lastLoadedPortal = HopperRoot.Get<PortalManager>().LastLoadedPortal;

			if (lastLoadedPortal == null)
				return;

            VRMLMetaInfos metaInfos;

			VRMLFile_MetaInfoProtocol metaData = lastLoadedPortal.GetMetaData();
            if (metaData != null)
                metaInfos = metaData.GetMetaInfos();
            else
            {
                metaInfos = new VRMLMetaInfos()
                {
                    m_Name = "UNKNOWN LOCATION", 
                    m_Description = lastLoadedPortal.Url
				};
            }

			TrackHistoryEntry entry = new()
			{
				Guid = Guid.Empty, //Guid.Parse(UserSettings.global.UUID),
				Link = HopperRoot.Get<PortalManager>().CurrentVrmlUrl,
				LastPosition = UserRig.Instance.LastGroundedPosition,
				LastOrientation = UserRig.Instance.LastGroundedForward,
				MetaInfos = metaInfos
			};

			if (TrackList.Count == 0)
				TrackList.Add(entry);

			TrackHistoryEntry lastEntry = TrackList[0];

			if (lastEntry.Link == entry.Link)
			{
				lastEntry.LastOrientation = entry.LastOrientation;
				lastEntry.LastPosition = entry.LastPosition;
			}
			else
			{
                Debug.Log($"Adding new entry {entry.Link} to track list");
                TrackList.Insert(0, entry);
			}
		}


		[CanBeNull]
        public TrackHistoryEntry Last()
        {
            if ( TrackList == null || TrackList.Count == 0 )
                return null;

            return TrackList[0];
        }

        public TrackHistoryEntry FindLink( string link )
        {
            for ( int i = 0; i < TrackList.Count; i++ )
            {
                if ( TrackList[i].Link == link )
                    return TrackList[i];
            }

            return null;
        }

        public bool GetPositionOrientationForUrl(string url, out Vector3 position, out Vector3 orientation)
        {
            position = Vector3.zero;
            orientation = Vector3.forward;
            TrackHistoryEntry entry = FindLink(url);
            
            if (entry == null)
                return false;

            position = entry.LastPosition;
            orientation = entry.LastOrientation;
            return true;
        }
        public void Save()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            JsonSerializerSettings settings =
                new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            string jsonFile = JsonConvert.SerializeObject( m_TrackList, settings );

            try
            {
                File.WriteAllText( FilePath, jsonFile, Encoding.UTF8 );
            }
            catch ( Exception )
            {
            }

            jsonFile = JsonConvert.SerializeObject( m_Favorites, settings );

            try
            {
                File.WriteAllText( FavoritesFilePath, jsonFile, Encoding.UTF8 );
            }
            catch ( Exception )
            {
            }

            CultureInfo.CurrentCulture = culture;
        }

        public bool Load()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                string jsonFile = File.ReadAllText( FilePath, Encoding.UTF8 );
                m_TrackList = JsonConvert.DeserializeObject < TrackList >( jsonFile );

                if ( m_TrackList == null || m_TrackList.History == null )
                {
                    m_TrackList = new();
                    m_TrackList.History = new();
                }
            }
            catch ( Exception )
            {
                return false;
            }

            try
            {
                string jsonFile = File.ReadAllText( FavoritesFilePath, Encoding.UTF8 );
                m_Favorites = JsonConvert.DeserializeObject < TrackList >( jsonFile );

                if ( m_Favorites == null || m_Favorites.History == null )
                {
                    m_Favorites = new();
                    m_Favorites.History = new();
                }
            }
            catch ( Exception )
            {
                return false;
            }

            IsLoadingTrackFile = false;
            CultureInfo.CurrentCulture = culture;

            return true;
        }

        public void AddToFavorites( TrackHistoryEntry entry )
        {
            RemoveFromFavorites( entry );
            Favorites.Insert( 0, entry );
        }

        public TrackHistoryEntry RemoveFromFavorites( TrackHistoryEntry entry )
        {
            TrackHistoryEntry alreadyExistsEntry = null;

            foreach ( TrackHistoryEntry favoriteEntry in Favorites )
            {
                if ( favoriteEntry.Link == entry.Link )
                {
                    alreadyExistsEntry = favoriteEntry;
                    break;
                }
            }

            if ( alreadyExistsEntry != null )
                Favorites.Remove( alreadyExistsEntry );

            return alreadyExistsEntry;
        }

        public static bool IsEqual( TrackHistoryEntry entry1, TrackHistoryEntry entry2 )
        {
            return entry1.Link == entry2.Link;
        }

        public bool IsInFavorites( TrackHistoryEntry entry )
        {
            foreach ( TrackHistoryEntry trackHistoryEntry in Favorites )
            {
                if ( IsEqual(entry, trackHistoryEntry ) )
                    return true;
            }

            return false;
        }
    }


    [Serializable]
    public class TrackList
    {
        public List < TrackHistoryEntry > History;
    }

    [Serializable]
    public class TrackHistoryEntry
    {
        public string Link;
        public VRMLMetaInfos MetaInfos;
        public SimpleVector3 LastPosition;
        public SimpleVector3 LastOrientation;
        public Guid Guid;
        public List < string > Tags = new();
    }
}