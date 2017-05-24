using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

namespace DesktopPictureFrame
{
  public partial class MainForm : Form
  {
    //-------------------------------------------------------------------------

    const int REFRESH_RATE_MINS = 10;
    const string SETTINGS_FILENAME = "settings.xml";
    const string LOG_FILENAME = "log.txt";

    enum MouseClickMode
    {
      NONE,
      SCROLL_LEFT,
      SCROLL_RIGHT,
      TOGGLE_WINDOW_BORDER
    }

    MouseClickMode ClickMode = MouseClickMode.NONE;
    string AppPath;
    StreamWriter LogFile;
    List<string> ImageLocations = new List<string>();
    int ActiveImageIndex;
    Thread RefreshRunner;
    bool RefreshRunnerIsAlive;

    //-------------------------------------------------------------------------

    public MainForm()
    {
      try
      {
        AppPath =
          Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) +
          Path.DirectorySeparatorChar;

        InitializeComponent();
      }
      catch( Exception ex )
      {
        if( LogFile != null )
        {
          Log( ex );
        }
        else
        {
          MessageBox.Show( 
            "Error while initialising: " + ex.Message,
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error );
        }
      }
    }

    //-------------------------------------------------------------------------

    private void MainForm_Load( object sender, EventArgs e )
    {
      try
      {
        InitialiseLog();
        LoadSettings();
        InitialiseRefreshThread();

        SwitchImage( 0 );
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    private void MainForm_FormClosing( object sender, FormClosingEventArgs e )
    {
      try
      {
        SaveSettings();
        ShutdownRefreshThread();
        ShutdownLog();
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void InitialiseLog()
    {
      try
      {
        LogFile = new StreamWriter( AppPath + LOG_FILENAME, true );

        Log( "Log initialised." );
      }
      catch( Exception ex )
      {
        MessageBox.Show(
          "Error while initialising log: " + ex.Message,
          "Error",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error );
      }
    }
    
    //-------------------------------------------------------------------------

    void ShutdownLog()
    {
      try
      {
        Log( "Bye." );

        if( LogFile != null )
        {
          LogFile.Close();
        }
      }
      catch( Exception )
      {
        // Ignore.
      }
    }

    //-------------------------------------------------------------------------

    void LoadSettings()
    {
      try
      {
        string settingsFilename = AppPath + SETTINGS_FILENAME;

        if( File.Exists( settingsFilename ) == false )
        {
          SaveSettings();
          return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load( settingsFilename );

        XmlElement settingsXml = xmlDoc.SelectSingleNode( "Settings" ) as XmlElement;

        if( settingsXml != null )
        {
          LoadImageLocationsFromXml( settingsXml );
          LoadWindowLocationFromXml( settingsXml );
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void LoadImageLocationsFromXml( XmlElement settingsXml )
    {
      try
      {
        ImageLocations.Clear();

        XmlElement collection =
          settingsXml.SelectSingleNode( "ImageCollection" ) as XmlElement;

        if( collection == null )
        {
          Log( "Image collection not found in settings." );
          return;
        }

        XmlNodeList locationsXml = collection.GetElementsByTagName( "Image" );

        foreach( XmlElement imageXml in locationsXml )
        {
          ImageLocations.Add( imageXml.InnerText );
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void LoadWindowLocationFromXml( XmlElement settingsXml )
    {
      try
      {
        XmlElement windowXml = settingsXml.SelectSingleNode( "Window" ) as XmlElement;

        if( windowXml == null )
        {
          return;
        }

        HandleClick_ToggleWindowBorder();

        int x =
          int.Parse(
            ( windowXml.SelectSingleNode( "X" ) as XmlElement ).InnerText );

        int y =
          int.Parse(
            ( windowXml.SelectSingleNode( "Y" ) as XmlElement ).InnerText );

        Width =
          int.Parse(
            ( windowXml.SelectSingleNode( "Width" ) as XmlElement ).InnerText );

        Height =
          int.Parse(
            ( windowXml.SelectSingleNode( "Height" ) as XmlElement ).InnerText );

        Location = new System.Drawing.Point( x, y );

        HandleClick_ToggleWindowBorder();
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void SaveSettings()
    {
      try
      {
        XmlDocument xmlDoc = new XmlDocument();

        XmlElement settingsXml = xmlDoc.CreateElement( "Settings" );
        xmlDoc.AppendChild( settingsXml );

        SaveImageLocationsToXml( settingsXml );
        SaveWindowLocationToXml( settingsXml );

        xmlDoc.Save( AppPath + SETTINGS_FILENAME );
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void SaveImageLocationsToXml( XmlElement settingsXml )
    {
      try
      {
        XmlElement collection =
          settingsXml.OwnerDocument.CreateElement( "ImageCollection" );
        
        settingsXml.AppendChild( collection );

        if( ImageLocations.Count == 0 )
        {
          ImageLocations.Add( "http://example.com/example.png" );
        }

        foreach( string location in ImageLocations )
        {
          XmlElement imageXml = settingsXml.OwnerDocument.CreateElement( "Image" );
          collection.AppendChild( imageXml );
          imageXml.InnerText = location;
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void SaveWindowLocationToXml( XmlElement settingsXml )
    {
      try
      {
        XmlElement windowXml =
          settingsXml.OwnerDocument.CreateElement( "Window" );
        
        settingsXml.AppendChild( windowXml );

        XmlElement xXml = settingsXml.OwnerDocument.CreateElement( "X" );
        windowXml.AppendChild( xXml );
        xXml.InnerText = Location.X.ToString();

        XmlElement yXml = settingsXml.OwnerDocument.CreateElement( "Y" );
        windowXml.AppendChild( yXml );
        yXml.InnerText = Location.Y.ToString();

        XmlElement wXml = settingsXml.OwnerDocument.CreateElement( "Width" );
        windowXml.AppendChild( wXml );
        wXml.InnerText = Width.ToString();

        XmlElement hXml = settingsXml.OwnerDocument.CreateElement( "Height" );
        windowXml.AppendChild( hXml );
        hXml.InnerText = Height.ToString();
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void InitialiseRefreshThread()
    {
      try
      {
        var threadStart = new ThreadStart( RunRefresh );
        RefreshRunner = new Thread( threadStart );
        RefreshRunner.Start();
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void ShutdownRefreshThread()
    {
      try
      {
        RefreshRunnerIsAlive = false;
        RefreshRunner.Join( 500 );
      }
      catch( Exception )
      {
        // Ignore.
      }
    }

    //-------------------------------------------------------------------------

    void Log( string msg )
    {
      try
      {
        if( LogFile != null )
        {
          LogFile.WriteLine(
            string.Format( "{0} | {1}",
              DateTime.Now.ToString( "yyyyMMdd HH:mm:ss" ),
              msg ) );

          LogFile.Flush();
        }
      }
      catch( Exception )
      {
        // Ignore.
      }
    }

    //-------------------------------------------------------------------------

    void Log( Exception ex )
    {
      Log( ex.Message );
    }

    //-------------------------------------------------------------------------

    void MainForm_MouseMove( object sender, MouseEventArgs e )
    {
      try
      {
        int leftThresholdX = (int)( Width * 0.33 );
        int rightThresholdX = (int)( Width - leftThresholdX );

        if( e.X < leftThresholdX )
        {
          ApplyScrollLeftMode();        
        }
        else if( e.X > rightThresholdX )
        {
          ApplyScrollRightMode();
        }
        else
        {
          ApplyToggleWindowBorderMode();
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void ApplyScrollLeftMode()
    {
      ClickMode = MouseClickMode.SCROLL_LEFT;
      Cursor = Cursors.PanWest;
    }

    //-------------------------------------------------------------------------

    void ApplyScrollRightMode()
    {
      ClickMode = MouseClickMode.SCROLL_RIGHT;
      Cursor = Cursors.PanEast;
    }

    //-------------------------------------------------------------------------

    void ApplyToggleWindowBorderMode()
    {
      ClickMode = MouseClickMode.TOGGLE_WINDOW_BORDER;
      Cursor = Cursors.Hand;
    }

    //-------------------------------------------------------------------------

    void uiImage_Click( object sender, EventArgs e )
    {
      try
      {
        switch( ClickMode )
        {
          case MouseClickMode.NONE:
            break;

          case MouseClickMode.SCROLL_LEFT:
            HandleClick_ScrollLeft();
            break;

          case MouseClickMode.SCROLL_RIGHT:
            HandleClick_ScrollRight();
            break;

          case MouseClickMode.TOGGLE_WINDOW_BORDER:
            if( ((MouseEventArgs)e).Button == MouseButtons.Right )
            {
              RefreshImage();
            }
            else
            {
              HandleClick_ToggleWindowBorder();
            }
            break;
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------
    
    void HandleClick_ScrollLeft()
    {
      try
      {
        ActiveImageIndex--;

        if( ActiveImageIndex < 0 )
        {
          ActiveImageIndex = ImageLocations.Count - 1;
        }

        SwitchImage( ActiveImageIndex );
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------
    
    void HandleClick_ScrollRight()
    {
      try
      {
        ActiveImageIndex++;

        if( ActiveImageIndex >= ImageLocations.Count )
        {
          ActiveImageIndex = 0;
        }

        SwitchImage( ActiveImageIndex );
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void HandleClick_ToggleWindowBorder()
    {
      try
      {
        if( FormBorderStyle == FormBorderStyle.None )
        {
          FormBorderStyle = FormBorderStyle.Sizable;
        }
        else
        {
          FormBorderStyle = FormBorderStyle.None;
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void SwitchImage( int index )
    {
      try
      {
        if( index < 0 || index >= ImageLocations.Count )
        {
          Log( string.Format( "Invalid index {0}.", index ) );
          return;
        }

        string path = ImageLocations[ index ];

        try
        {
          uiImage.ImageLocation = path;
        }
        catch( Exception )
        {
          Log( string.Format( "Failed to load image \"{0}\"", path ) );

          index++;
          if( index >= ImageLocations.Count )
          {
            index = 0;
          }

          SwitchImage( index );
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    delegate void RefreshImageDelegate();

    void RefreshImage()
    {
      try
      {
        Cursor = Cursors.WaitCursor;

        uiImage.Load();
        //uiImage.Refresh();

        Cursor = Cursors.Default;
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------

    void RunRefresh()
    {
      try
      {
        RefreshImageDelegate refreshDelegate =
          new RefreshImageDelegate( RefreshImage );

        RefreshRunnerIsAlive = true;

        while( RefreshRunnerIsAlive )
        {
          try
          {
            const int numberOfSecsPerRefresh = REFRESH_RATE_MINS * 60;

            for( int i = 0; i < numberOfSecsPerRefresh; i++ )
            {
              Thread.Sleep( 1000 );

              if( RefreshRunnerIsAlive == false )
              {
                break;
              }
            }
          }
          catch( Exception )
          {
            // Ignore.
          }

          Invoke( refreshDelegate );
        }
      }
      catch( Exception ex )
      {
        Log( ex );
      }
    }

    //-------------------------------------------------------------------------
  }
}
