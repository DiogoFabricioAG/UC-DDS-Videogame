using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class Game
{
    private View _view;
    public Game(View view, string teamsFolder)
    {
        _view = view;
    }
    
    public void Play()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
    }
}
