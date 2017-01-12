using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;

namespace FileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            WebReference.WSCtrlPc ws = new WebReference.WSCtrlPc();
            Object Guid = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", null);
            Console.WriteLine("Lancement de FileManager");
            string path = @"C:\ProgramData\CtrlPc\";
            string pathLog = @"C:\ProgramData\CtrlPc\LOG\";
            string linkDownload = ConfigurationManager.AppSettings["linkDownload"];

            string[] pathDef = { path + @"SCRIPT\", path + @"FLAG\", path + @"PLANNING\", path + @"INSTALLATION\" };

            foreach (string pathRep in pathDef)
            {
                string[] file = Directory.GetFiles(pathRep);
                foreach (string nameFile in file)
                {
                    if (ws.GetPresenceFile(Guid.ToString(),nameFile).Contains("KO"))
                    {
                        Console.WriteLine("Suppression du fichier "+nameFile);
                        try
                        {
                            File.Delete(nameFile);
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(err.Message);
                        }
                    }
                    else if (ws.GetPresenceFile(Guid.ToString(),nameFile).Contains("MAJ"))
                    {
                        Console.WriteLine("La présence du fichier "+nameFile+ " a été ajoutée");
                    }
                    
                }
            }

            string[] fileLog = Directory.GetFiles(pathLog);
            long Size=0;
            foreach (string log in fileLog)
            {
                FileInfo infoLog = new FileInfo(log);
                if (infoLog.LastWriteTime< DateTime.Now.AddDays(-7))
                {
                    Console.WriteLine("Suppression du fichier :" + log + " Dernière écriture le : " + infoLog.LastWriteTime);
                    try
                    {
                        File.Delete(log);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                    }
                }
                else
                {
                    Size = Size + infoLog.Length;
                }
                
            }
            if (Size>100000000)
            {
                Console.WriteLine("Suppression de l'ensemble de fichier sous LOG // SIZE : " + Size.ToString());
                foreach (string log in fileLog)
                {
                    FileInfo infoLog = new FileInfo(log);
                    Console.WriteLine("Suppression du fichier :" + log + " // Taille : " + infoLog.Length);
                    try
                    {
                        File.Delete(log);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);                        
                    }
                    
                }
            }
            else
            {
                Console.WriteLine("Taille du répertoire LOG : " + Size.ToString());
            }

            //gestion de l'auto upload

            string autoUpload = ws.GetAutoUpload(Guid.ToString());
            if (!autoUpload.Contains("RAS"))
            {
                if (autoUpload.Length>5)
                {
                    Console.WriteLine("Il manque un ou plusieurs fichiers");
                    try
                    {
                        string[] files = autoUpload.Split('!');
                        foreach (string file in files)
                        {
                            if (file.Length>3)
                            {
                                Console.WriteLine("Début du téléchargement de : "+file);
                                try
                                {
                                    string[] filePath = file.Split(';');
                                    string uri = linkDownload + filePath[1];
                                    string dest = @"C:\ProgramData\CtrlPc\" + filePath[0] + @"\" + filePath[1];
                                    WebClient webClient = new WebClient();
                                    webClient.DownloadFile(new Uri(uri), dest);
                                    Console.WriteLine("Fin du téléchargement de : " + file);
                                }
                                catch (Exception err)
                                {
                                    Console.WriteLine("Erreur de téléchargement de : " + file);
                                    Console.WriteLine(err.Message);
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("Erreur sur le split de la ligne : " + autoUpload);
                        Console.WriteLine(err.Message);
                    }
                    
                }
            }
            else
            {
                Console.WriteLine("Tous les fichiers sont présent");
            }

            Console.WriteLine("Traiment FileManager terminé");
            Console.ReadLine();
        }
    }
}
