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

                    Console.WriteLine("Clicca U se sei utente e A se sei Admin:");
                    string userType = Console.ReadLine();

                    if (userType.ToUpper() == "U")
                    {
                        Utenti utenteAutenticato = Login(context, banca);
                        if (utenteAutenticato != null)
                        {
                            Console.WriteLine($"Benvenuto, {utenteAutenticato.NomeUtente}!");
                            continua = MenuPrincipale(context, utenteAutenticato, banca);
                        }
                        else
                        {
                            Console.WriteLine("Accesso negato. Utente bloccato o dati di accesso errati.");
                        }
                    }
                    else if (userType.ToUpper() == "A")
                    {
                        Admin adminAutenticato = LoginAdmin(context, banca);
                        if (adminAutenticato != null)
                        {
                            Console.WriteLine($"Benvenuto, {adminAutenticato}!");
                            continua = AdminMenu(context, banca);
                        }
                        else
                        {
                            Console.WriteLine("Accesso negato. Admin bloccato o dati di accesso errati.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Selezione non valida. Riprova.");
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
            Console.ReadKey();
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
                        contoCorrente.Saldo += importo;

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
                    var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

                    if (contoCorrente != null)
                    {
                        if (contoCorrente.Saldo >= importo)
                        {
                            contoCorrente.Saldo -= importo;

                            contoCorrente.DataUltimaOperazione = DateTime.Now;

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
            // Verifico se la banca dell'utente ha la funzionalità "Registro Operazioni"
            var banca = context.Banches.FirstOrDefault(b => b.Id == utenteAutenticato.IdBanca);

            if (banca != null && HaFunzionalitaRegistroOperazioni(context, banca))
            {
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
            // Verifico se la banca ha la funzionalità "Registro Operazioni"
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
