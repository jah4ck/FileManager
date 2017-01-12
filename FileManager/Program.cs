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
            Trace trace = new Trace();
            Object Guid = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", null);
            trace.WriteLog("Lancement de FileManager",2);

            string path = @"C:\ProgramData\CtrlPc\";
            string pathLog = @"C:\ProgramData\CtrlPc\LOG\";
            string linkDownload = ConfigurationManager.AppSettings["linkDownload"];

            string[] pathDef = { path + @"SCRIPT\", path + @"FLAG\", path + @"PLANNING\", path + @"INSTALLATION\" };
            trace.WriteLog("reset presence fichier", 2);
            try
            {
                ws.SetResetPresence(Guid.ToString());
                trace.WriteLog("reset presence fichier OK", 2);
            }
            catch (Exception err)
            {
                trace.WriteLog("reset presence KO", 1);
                trace.WriteLog(err.Message, 1);
            }

            foreach (string pathRep in pathDef)
            {
                string[] file = Directory.GetFiles(pathRep);
                foreach (string nameFile in file)
                {
                    try
                    {
                        if (ws.GetPresenceFile(Guid.ToString(),nameFile).Contains("KO"))
                        {
                            trace.WriteLog("Suppression du fichier "+nameFile,2);
                            try
                            {
                                File.Delete(nameFile);
                            }
                            catch (Exception err)
                            {
                                trace.WriteLog(err.Message,1);
                            }
                        }
                        else if (ws.GetPresenceFile(Guid.ToString(),nameFile).Contains("MAJ"))
                        {
                            trace.WriteLog("La présence du fichier "+nameFile+ " a été ajoutée",2);
                        }
                    }
                    catch (Exception err)
                    {
                        trace.WriteLog("Erreur : "+err.Message, 1);
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
                    trace.WriteLog("Suppression du fichier :" + log + " Dernière écriture le : " + infoLog.LastWriteTime,2);
                    try
                    {
                        File.Delete(log);
                    }
                    catch (Exception err)
                    {
                        trace.WriteLog(err.Message,1);
                    }
                }
                else
                {
                    Size = Size + infoLog.Length;
                }
                
            }
            if (Size>100000000)
            {
                trace.WriteLog("Suppression de l'ensemble de fichier sous LOG (quota dépassé) // SIZE : " + Size.ToString(),2);
                foreach (string log in fileLog)
                {
                    FileInfo infoLog = new FileInfo(log);
                    trace.WriteLog("Suppression du fichier :" + log + " // Taille : " + infoLog.Length,2);
                    try
                    {
                        File.Delete(log);
                    }
                    catch (Exception err)
                    {
                        trace.WriteLog(err.Message,1);                        
                    }
                    
                }
            }
            else
            {
                trace.WriteLog("Taille du répertoire LOG : " + Size.ToString(),2);
            }

            //gestion de l'auto upload
            string autoUpload="";
            try
            {
                autoUpload = ws.GetAutoUpload(Guid.ToString());
            }
            catch (Exception err)
            {
                trace.WriteLog("Erreur : "+err.Message, 1);
            }

            if (!autoUpload.Contains("RAS"))
            {
                if (autoUpload.Length>5)
                {
                    trace.WriteLog("Il manque un ou plusieurs fichiers",2);
                    try
                    {
                        string[] files = autoUpload.Split('!');
                        foreach (string file in files)
                        {
                            if (file.Length>3)
                            {
                                trace.WriteLog("Début du téléchargement de : "+file,2);
                                try
                                {
                                    string[] filePath = file.Split(';');
                                    string uri = linkDownload + filePath[1];
                                    string dest = @"C:\ProgramData\CtrlPc\" + filePath[0] + @"\" + filePath[1];
                                    WebClient webClient = new WebClient();
                                    webClient.DownloadFile(new Uri(uri), dest);
                                    trace.WriteLog("Fin du téléchargement de : " + file,2);
                                }
                                catch (Exception err)
                                {
                                    trace.WriteLog("Erreur de téléchargement de : " + file,1);
                                    trace.WriteLog(err.Message,1);
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        trace.WriteLog("Erreur sur le split de la ligne : " + autoUpload,1);
                        trace.WriteLog(err.Message,1);
                    }
                    
                }
            }
            else
            {
                trace.WriteLog("Tous les fichiers sont présent",2);
            }

            trace.WriteLog("Traiment FileManager terminé",2);
        }
    }
}
