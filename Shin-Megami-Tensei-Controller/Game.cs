using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class Game
{
    private View _view;
    private string _teamsFolder;
    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
    }
    
    public void Play()
    {
        
        _view.WriteLine("Elige un archivo para cargar los equipos");
        if (Directory.Exists(_teamsFolder))
        {
            try
            {
                string[] archivos = Directory.GetFiles(_teamsFolder);
                int contador = 0;
                foreach (string archivo in archivos)
                {
                    _view.WriteLine($"{contador}: {Path.GetFileName(archivo)}");
                    contador++;
                }
                
            }
            catch (Exception ex)
            {
                _view.WriteLine($"Ocurrió un error al leer la carpeta: {ex.Message}");
            }
        }
        else
        {
            _view.WriteLine("La carpeta de equipos no existe.");
        }
        var input = _view.ReadLine();
        var seleccion = Convert.ToInt32(input);
        
        var archivoSeleccionado= Directory.GetFiles(_teamsFolder)[seleccion];
        var lines = File.ReadAllLines(archivoSeleccionado);
        
        var partida = new Partida();
        var result = partida.CreacionEquipo(lines);
        _view.WriteLine(result);
        
        
        
    }
}
