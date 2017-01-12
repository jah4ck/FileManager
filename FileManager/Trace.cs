using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using TraceLogAsync;

namespace FileManager
{
    public class Trace
    {
        private int status = 3;//fichier
        private int statusParam = 3;//fichier
        private int type = 3;//tout
        public void WriteLog(string message, int codeErreur)
        {
            string CodeAppli = ConfigurationManager.AppSettings["CodeAppli"];
            LogWriter write = LogWriter.Instance;
            if (File.Exists(@"c:\ProgramData\CtrlPc\SCRIPT\RemLog.nfo"))
            {
                using (FileStream filestream = new FileStream(@"c:\ProgramData\CtrlPc\SCRIPT\RemLog.nfo", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader read = new StreamReader(filestream))
                    {
                        string ligne;
                        while ((ligne = read.ReadLine()) != null)
                        {
                            if (ligne.Length > 2)
                            {
                                string[] colonne = ligne.Split(';');
                                Int32.TryParse(colonne[0], out status);
                                Int32.TryParse(colonne[1], out statusParam);
                                Int32.TryParse(colonne[2], out type);
                            }
                        }
                    }
                }
            }

            SynchroHeure MySynchroHeure = new SynchroHeure();
            DateTime dateTraitement = DateTime.Now;
            try
            {
                dateTraitement = MySynchroHeure.GetNetworkTime();
            }
            catch (Exception err)
            {
                dateTraitement = DateTime.Now;
            }

            if (status == 3 && statusParam == 3)//mode journal
            {
                try
                {

                    if (codeErreur == 1 && (type == 3 || type == 1))
                    {
                        write.WriteToLog(message.ToString(), codeErreur, "JOURNAL");
                    }
                    if (codeErreur == 2 && (type == 3 || type == 2))
                    {
                        write.WriteToLog(message.ToString(), codeErreur, "JOURNAL");
                    }

                }
                catch (Exception err)
                {
                    write.WriteToLog(err.Message, 1, "JOUNRAL_ERREUR");
                }

            }
            else if (status == 2 && statusParam == 2) //mode WS
            {
                try
                {
                    Object Guid = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", null);
                    WebReference.WSCtrlPc ws = new WebReference.WSCtrlPc();
                    string result = "OK";
                    if (codeErreur == 1 && (type == 3 || type == 1))
                    {
                        result = ws.TraceLogNew(Guid.ToString(), dateTraitement, CodeAppli, codeErreur, message);
                    }
                    if (codeErreur == 2 && (type == 3 || type == 2))
                    {
                        result = ws.TraceLogNew(Guid.ToString(), dateTraitement, CodeAppli, codeErreur, message);
                    }
                    if (result == "RELICA")
                    {
                        if (codeErreur == 1 && (type == 3 || type == 1))
                        {
                            write.WriteToLog(message.ToString(), codeErreur, "RELICA");
                        }
                        if (codeErreur == 2 && (type == 3 || type == 2))
                        {
                            write.WriteToLog(message.ToString(), codeErreur, "RELICA");
                        }
                    }
                }
                catch (Exception err)
                {
                    write.WriteToLog(err.Message, codeErreur, "JOURNAL_ERREUR");
                    if (codeErreur == 1 && (type == 3 || type == 1))
                    {
                        write.WriteToLog(message.ToString(), codeErreur, "JOURNAL_ERREUR");
                    }
                    if (codeErreur == 2 && (type == 3 || type == 2))
                    {
                        write.WriteToLog(message.ToString(), codeErreur, "JOURNAL_ERREUR");
                    }

                }
            }

            if (status == 3 && statusParam == 2) //mode relica
            {
                try
                {
                    WebReference.WSCtrlPc ws = new WebReference.WSCtrlPc();
                    Object Guid = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", null);

                    if (codeErreur == 1 && (type == 3 || type == 1))
                    {
                        write.WriteToLog(message.ToString(), codeErreur, "RELICA");
                        ws.SetIncrementeRelica(Guid.ToString());
                    }
                    if (codeErreur == 2 && (type == 3 || type == 2))
                    {
                        write.WriteToLog(message.ToString(), codeErreur, "RELICA");
                        ws.SetIncrementeRelica(Guid.ToString());
                    }


                }
                catch (Exception err)
                {
                    write.WriteToLog(err.Message, codeErreur, "RELICA_ERREUR");
                }
            }


        }
    }
}
