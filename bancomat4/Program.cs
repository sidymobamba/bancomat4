using System;
using System.Collections.Generic;
using System.Linq;
using bancomat4;

namespace bancomat2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var context = new bancomat2Entities())
            {
                Console.WriteLine("Benvenuto al programma Bankomat!");

                bool continua = true;

                while (continua)
                {
                    Console.Clear();
                    Banche banca = SelezionaBanca(context);
                    Utenti utenteAutenticato = Login(context, banca);
                    Admin adminAutenticato = LoginAdmin(context, banca);


                    if (utenteAutenticato != null)
                    {
                        Console.WriteLine($"Benvenuto, {utenteAutenticato.NomeUtente}!");

                        // Entra nel menu principale
                        continua = MenuPrincipale(context, utenteAutenticato, banca);
                    }
                    else if (adminAutenticato != null)
                    {
                        Console.WriteLine($"Benvenuto, {adminAutenticato.NomeUtente}!");      
                        continua = AdminMenu(context, banca);
                    }
                    else
                    {
                        Console.WriteLine("Accesso negato. Utente bloccato o dati di accesso errati.");
                    }
                }
            }

            Console.WriteLine("Grazie per aver utilizzato Bankomat. Arrivederci!");
            Console.ReadKey();
        }

        static Banche SelezionaBanca(bancomat2Entities context)
        {
            Console.WriteLine("\nSeleziona una banca:");
            MostraBancheDisponibili(context);

            Console.Write("Nome Banca: ");
            string nomeBanca = Console.ReadLine();

            Banche bancaSelezionata = TrovaBancaPerNome(context, nomeBanca);
            if (bancaSelezionata == null)
            {
                Console.WriteLine("Banca non valida. Riprova.");
                return SelezionaBanca(context);
            }

            return bancaSelezionata;
        }

        static Utenti Login(bancomat2Entities context, Banche bancaSelezionata)
        {
            int tentativiRimasti = 3;

            Console.Write("Nome Utente: ");
            string nomeUtente = Console.ReadLine();
            Utenti utente = SelezionaUtente(context, bancaSelezionata, nomeUtente);

            if (utente == null)
            {
                Console.WriteLine("Utente non trovato.");
                return null;
            }

            while (!utente.Bloccato && tentativiRimasti > 0)
            {
                Console.Write("Password: ");
                string password = Console.ReadLine();

                if (AutenticaUtente(utente, password))
                {
                    return utente;
                }
                else
                {
                    tentativiRimasti--;
                    Console.WriteLine($"Tentativi rimasti: {tentativiRimasti}");
                }
            }

            Console.WriteLine("Utente bloccato. Contatta l'assistenza.");
            utente.Bloccato = true;
            context.SaveChanges();

            return null;
        }

        static Admin LoginAdmin(bancomat2Entities context, Banche bancaSelezionata)
        {
            Console.Write("Nome Utente (Admin): ");
            string nomeUtente = Console.ReadLine();
            Admin admin = SelezionaAdmin(context, bancaSelezionata, nomeUtente);

            if (admin == null)
            {
                Console.WriteLine("Admin non trovato.");
                return null;
            }

            Console.Write("Password (Admin): ");
            string password = Console.ReadLine();

            if (AutenticaAdmin(admin, password))
            {
                return admin;
            }
            else
            {
                Console.WriteLine("Password errata.");
                return null;
            }
        }



        static void MostraBancheDisponibili(bancomat2Entities context)
        {
            var banche = context.Banches.ToList();

            foreach (var banca in banche)
            {
                Console.WriteLine($"- {banca.Nome}");
            }
        }

        static Banche TrovaBancaPerNome(bancomat2Entities context, string nomeBanca)
        {
            return context.Banches.FirstOrDefault(b => b.Nome.Equals(nomeBanca, StringComparison.OrdinalIgnoreCase));
        }

        static Utenti SelezionaUtente(bancomat2Entities context, Banche banca, string nomeUtente)
        {
            return context.Utentis
                    .FirstOrDefault(u => u.IdBanca == banca.Id &&
                    u.NomeUtente.Equals(nomeUtente, StringComparison.OrdinalIgnoreCase));
        }
        static Admin SelezionaAdmin(bancomat2Entities context, Banche banca, string nomeUtente)
        {
            return context.Admins
                    .FirstOrDefault(u => u.IdBanca == banca.Id &&
                    u.NomeUtente.Equals(nomeUtente, StringComparison.OrdinalIgnoreCase));
        }

        static bool AutenticaUtente(Utenti utente, string password)
        {
            return utente.Password == password;
        }
        static bool AutenticaAdmin(Admin admin, string password)
        {
            return admin.Password == password;
        }

        static string SelezionaBanca()
        {
            Console.WriteLine("Benvenuto a Bankomat!");
            Console.WriteLine("Seleziona una banca:");

            using (var context = new bancomat2Entities())
            {
                var banche = context.Banches.Select(b => b.Nome).ToList();

                for (int i = 0; i < banche.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {banche[i]}");
                }

                int scelta;
                if (int.TryParse(Console.ReadLine(), out scelta) && scelta >= 1 && scelta <= banche.Count)
                {
                    return banche[scelta - 1];
                }
            }

            return null;
        }

        static bool AutenticazioneUtente(bancomat2Entities context, string nomeBanca)
        {
            Console.WriteLine("Inserisci il tuo nome utente:");
            string nomeUtente = Console.ReadLine();
            Console.WriteLine("Inserisci la tua password:");
            string password = Console.ReadLine();

            var utente = context.Utentis.FirstOrDefault(u => u.NomeUtente == nomeUtente && u.Banche.Nome == nomeBanca);
            var admin = context.Admins.FirstOrDefault(u => u.NomeUtente == nomeUtente && u.Banche.Nome == nomeBanca);

            if (utente != null || admin != null)
            {
                if (!utente.Bloccato)
                {
                    if (utente.Password == password || admin.Password == password)
                    {
                        Console.WriteLine("Accesso riuscito. Benvenuto!");
                        return true; // Utente autenticato con successo
                    }
                    else
                    {
                        Console.WriteLine("Password errata. Riprova.");
                        // Aggiorna il conteggio dei tentativi falliti per l'utente
                        utente.TentativiFalliti++;
                        if (utente.TentativiFalliti >= 3)
                        {
                            Console.WriteLine("Hai superato il limite di tentativi falliti. L'utente verrà bloccato.");
                            utente.Bloccato = true;
                        }
                        context.SaveChanges(); 
                    }
                }
                else
                {
                    Console.WriteLine("L'utente è bloccato. Contatta l'assistenza.");
                }
            }
            else
            {
                Console.WriteLine("Utente non trovato. Riprova.");
            }

            return false; // Autenticazione fallita
        }
        static bool MenuPrincipale(bancomat2Entities context, Utenti utenteAutenticato, Banche banca)
        {
            long[] funzionalita = ServiziDisponibili(context, banca);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\nMenu Principale:");
                DisplayMenuItem(1, "Versamento", funzionalita);
                DisplayMenuItem(2, "Mostra Report Saldo", funzionalita);
                DisplayMenuItem(3, "Prelievo", funzionalita);
                DisplayMenuItem(4, "Mostra Registro Operazioni", funzionalita);
                Console.WriteLine("5. Logout");
                Console.WriteLine("6. Esci");

                Console.Write("Seleziona un'opzione: ");
                string scelta = Console.ReadLine();

                if (int.TryParse(scelta, out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            if (IsServiceAvailable(1, funzionalita))
                                EseguiVersamento(context, utenteAutenticato);
                            else
                                ShowInvalidOptionMessage();
                            break;
                        case 2:
                            if (IsServiceAvailable(2, funzionalita))
                                MostraReportSaldo(context, utenteAutenticato);
                            else
                                ShowInvalidOptionMessage();
                            break;
                        case 3:
                            if (IsServiceAvailable(3, funzionalita))
                                EseguiPrelievo(context, utenteAutenticato);
                            else
                                ShowInvalidOptionMessage();
                            break;
                        case 4:
                            if (IsServiceAvailable(4, funzionalita))
                                MostraRegistroOperazioni(context, utenteAutenticato);
                            else
                                ShowInvalidOptionMessage();
                            break;
                        case 5:
                            Console.WriteLine("Logout effettuato.");
                            return true;
                        case 6:
                            Console.WriteLine("Arrivederci!");
                            Console.ReadKey();
                            return false;
                        default:
                            ShowInvalidOptionMessage();
                            break;
                    }
                }
                else
                {
                    ShowInvalidOptionMessage();
                }
            }
        }

        static void DisplayMenuItem(int optionNumber, string description, long[] availableServices)
        {
            if (IsServiceAvailable(optionNumber, availableServices))
                Console.WriteLine($"{optionNumber}. {description}");
        }

        static bool IsServiceAvailable(int serviceNumber, long[] availableServices)
        {
            return availableServices.Contains(serviceNumber);
        }

        static void ShowInvalidOptionMessage()
        {
            Console.WriteLine("Opzione non valida. Riprova.");
        }

        static long[] ServiziDisponibili(bancomat2Entities context, Banche banca)
        {
            return context.Banche_Funzionalita
                          .Where(bf => bf.IdBanca == banca.Id)
                          .Select(bf => bf.IdFunzionalita)
                          .ToArray();
        }

        static void MostraMenuPrincipale(bancomat2Entities context, Utenti utenteAutenticato)
        {
            while (true)
            {
                Console.WriteLine("\nMenu Principale:");
                Console.WriteLine("1. Versamento");
                Console.WriteLine("2. Report Saldo");
                Console.WriteLine("3. Prelievo");
                Console.WriteLine("4. Registro Operazioni");
                Console.WriteLine("5. Logout");
                Console.Write("Scelta: ");

                if (int.TryParse(Console.ReadLine(), out int scelta))
                {
                    switch (scelta)
                    {
                        case 1:
                            EseguiVersamento(context, utenteAutenticato);
                            break;
                        case 2:
                            MostraReportSaldo(context, utenteAutenticato);
                            break;
                        case 3:
                            EseguiPrelievo(context, utenteAutenticato);
                            break;
                        case 4:
                            MostraRegistroOperazioni(context, utenteAutenticato);
                            break;
                        case 5:
                            Console.WriteLine("Logout effettuato.");
                            return; // Uscita dal menu principale
                        default:
                            Console.WriteLine("Scelta non valida.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Scelta non valida.");
                }
            }
        }

        static void EseguiVersamento(bancomat2Entities context, Utenti utenteAutenticato)
        {
            Console.Write("Inserisci l'importo da versare: ");
            if (int.TryParse(Console.ReadLine(), out int importo))
            {
                if (importo > 0)
                {
                    // Cerca il conto corrente dell'utente nel database
                    var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

                    if (contoCorrente != null)
                    {
                        // Aggiorna il saldo del conto corrente
                        contoCorrente.Saldo += importo;

                        // Aggiorna la data dell'ultima operazione
                        contoCorrente.DataUltimaOperazione = DateTime.Now;

                        // Registra l'operazione nel database
                        var movimento = new Movimenti
                        {
                            NomeBanca = utenteAutenticato.Banche.Nome,
                            NomeUtente = utenteAutenticato.NomeUtente,
                            Funzionalita = "Versamento",
                            Quantita = importo,
                            DataOperazione = DateTime.Now
                        };

                        context.Movimentis.Add(movimento);
                        context.SaveChanges();

                        Console.WriteLine($"Versamento di {importo} effettuato con successo.");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Conto corrente non trovato.");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("L'importo deve essere maggiore di zero.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Importo non valido.");
                Console.ReadKey();
            }
        }

        static void MostraReportSaldo(bancomat2Entities context, Utenti utenteAutenticato)
        {
            // Cerca il conto corrente dell'utente nel database
            var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

            if (contoCorrente != null)
            {
                Console.WriteLine("\nReport Saldo:");
                Console.WriteLine($"Saldo attuale: {contoCorrente.Saldo}");
                Console.WriteLine($"Data ultimo versamento: {contoCorrente.DataUltimaOperazione}");
                Console.WriteLine($"Data/ora attuale: {DateTime.Now}");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Conto corrente non trovato.");
                Console.ReadKey();
            }
        }

        static void EseguiPrelievo(bancomat2Entities context, Utenti utenteAutenticato)
        {
            Console.Write("Inserisci l'importo da prelevare: ");
            if (int.TryParse(Console.ReadLine(), out int importo))
            {
                if (importo > 0)
                {
                    // Cerca il conto corrente dell'utente nel database
                    var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

                    if (contoCorrente != null)
                    {
                        if (contoCorrente.Saldo >= importo)
                        {
                            // Esegui il prelievo
                            contoCorrente.Saldo -= importo;

                            // Aggiorna la data dell'ultima operazione
                            contoCorrente.DataUltimaOperazione = DateTime.Now;

                            // Registra l'operazione nel database
                            var movimento = new Movimenti
                            {
                                NomeBanca = utenteAutenticato.Banche.Nome,
                                NomeUtente = utenteAutenticato.NomeUtente,
                                Funzionalita = "Prelievo",
                                Quantita = importo,
                                DataOperazione = DateTime.Now
                            };

                            context.Movimentis.Add(movimento);
                            context.SaveChanges();

                            Console.WriteLine($"Prelievo di {importo} effettuato con successo.");
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.WriteLine("Saldo insufficiente per il prelievo.");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Conto corrente non trovato.");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("L'importo deve essere maggiore di zero.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Importo non valido.");
                Console.ReadKey();
            }
        }

        static void MostraRegistroOperazioni(bancomat2Entities context, Utenti utenteAutenticato)
        {
            // Verifica se la banca dell'utente ha la funzionalità "Registro Operazioni"
            var banca = context.Banches.FirstOrDefault(b => b.Id == utenteAutenticato.IdBanca);

            if (banca != null && HaFunzionalitaRegistroOperazioni(context, banca))
            {
                // Cerca tutte le operazioni dell'utente nel database
                var operazioni = context.Movimentis
                    .Where(m => m.NomeBanca == utenteAutenticato.Banche.Nome && m.NomeUtente == utenteAutenticato.NomeUtente)
                    .ToList();

                if (operazioni.Count > 0)
                {
                    Console.WriteLine("\nRegistro delle Operazioni:");
                    foreach (var operazione in operazioni)
                    {
                        Console.WriteLine($"Data operazione: {operazione.DataOperazione}");
                        Console.WriteLine($"Utente: {operazione.NomeUtente}");
                        Console.WriteLine($"Operazione: {operazione.Funzionalita}");
                        Console.WriteLine($"Importo: {operazione.Quantita}");
                        Console.WriteLine("--------------------------");
                    }
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Nessuna operazione registrata.");
                }
            }
            else
            {
                Console.WriteLine("La tua banca non supporta la funzionalità Registro Operazioni.");
            }
        }

        static bool HaFunzionalitaRegistroOperazioni(bancomat2Entities context, Banche banca)
        {
            // Verifica se la banca ha la funzionalità "Registro Operazioni"
            return context.Banche_Funzionalita
                .Any(bf => bf.IdBanca == banca.Id && bf.Funzionalita.Nome == "Registro Operazioni");
        }

        static bool AdminMenu(bancomat2Entities context, Banche banca)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Benvenuto Admin!");
                Console.WriteLine("1. Sblocca Utente Bloccato");
                Console.WriteLine("2. Lista Utenti della Banca");
                Console.WriteLine("3. Logout");
                Console.Write("Seleziona un'opzione: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            SbloccaUtenteBloccato(context, banca);
                            break;
                        case 2:
                            ListaUtentiBanca(context, banca);
                            break;
                        case 3:
                            Console.WriteLine("Logout effettuato.");
                            return false; 
                        default:
                            ShowInvalidOptionMessage();
                            break;
                    }
                }
                else
                {
                    ShowInvalidOptionMessage();
                }
            }
        }


        static void SbloccaUtenteBloccato(bancomat2Entities context, Banche banca)
        {
            Console.Write("Inserisci il nome utente dell'utente bloccato: ");
            string nomeUtente = Console.ReadLine();

            var utente = context.Utentis
                .FirstOrDefault(u => u.IdBanca == banca.Id && u.NomeUtente.Equals(nomeUtente, StringComparison.OrdinalIgnoreCase));

            if (utente != null && utente.Bloccato)
            {
                utente.Bloccato = false;
                context.SaveChanges();
                Console.WriteLine("Utente sbloccato con successo.");
            }
            else
            {
                Console.WriteLine("Utente non trovato o già sbloccato.");
            }

            Console.ReadKey();
        }

        static void ListaUtentiBanca(bancomat2Entities context, Banche banca)
        {
            var utentiBanca = context.Utentis
                .Where(u => u.IdBanca == banca.Id)
                .Select(u => u.NomeUtente)
                .ToList();

            Console.WriteLine("\nLista degli Utenti della Banca:");
            foreach (var utente in utentiBanca)
            {
                Console.WriteLine(utente);
            }

            Console.ReadKey();
        }

    }
}
