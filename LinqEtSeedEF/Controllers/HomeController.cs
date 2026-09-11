using LinqEtSeedEF.Data;
using LinqEtSeedEF.Models;
using LinqEtSeedEF.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqEtSeedEF.Controllers
{
    public class HomeController : Controller
    {
        private readonly LinqEtSeedEFContext _context;

        public HomeController(LinqEtSeedEFContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Data()
        {
            DataViewModel dataViewModel = new DataViewModel();

            dataViewModel.Clients = await _context.Client.ToListAsync();
            dataViewModel.Commandes = await _context.Commande.ToListAsync();
            dataViewModel.CommandePlats = await _context.CommandePlat.OrderBy(cp => cp.CommandeId).ToListAsync();
            dataViewModel.Plats = await _context.Plat.ToListAsync();
            dataViewModel.Restaurants = await _context.Restaurant.ToListAsync();

            return View(dataViewModel);
        }

        public async Task<IActionResult> Questions()
        {
            QuestionsViewModel questionViewModel = new QuestionsViewModel();

            // ATTENTION: N'enlevez pas ces lignes de code qui semblent peut-être inutiles.
            // Nous allons parler de loading au prochain cours et nous allons voir une comment gérer le loading efficacement.
            // D'ici là, comprenez simplement que ces lignes load TOUTES les données des tables et les gardent en mémoire pour la durée de la requête.
            // Normalement, on ne veut PAS travailler de cette manière!
            //Début du code qu'il faut garder
            await _context.Client.ToListAsync();
            await _context.Commande.ToListAsync();
            await _context.CommandePlat.ToListAsync();
            await _context.Plat.ToListAsync();
            await _context.Restaurant.ToListAsync();
            //Fin du code qu'il faut garder

            questionViewModel.PrixPlatLePlusCher = PrixPlatLePlusCher();
            questionViewModel.ValeurTotalDesPlats = ValeurTotalDesPlats();
            questionViewModel.ValeurTotalDesCommandes = ValeurTotalDesCommandes("Patrick Gagné");
            questionViewModel.PrixCommandeLaPlusCher = PrixCommandeLaPlusCher();

            questionViewModel.VegetarienResto1 = Vegetarien("La graine du père George");
            questionViewModel.VegetarienResto2 = Vegetarien("Le Bistro");
            questionViewModel.VegetarienResto3 = Vegetarien("La Belle Province");

            questionViewModel.PlatsVege = PlatsVegeOrdeCroissantDePrix();
            questionViewModel.PlatsLesPlusChers = PlatsLesPlusChersOrdeDecroissantDePrix(3);

            return View(questionViewModel);
        }

        private DecimalViewModel PrixPlatLePlusCher()
        {
            // TODO: Écrire la logique pour trouver le prix du plat le plus cher avec une boucle
            var liste = _context.Plat.ToList();
            decimal prix = 0;
            foreach (var plat in liste)
            {
                if (plat.Prix > prix)
                    prix = plat.Prix;
            }
  
            // TODO: Écrire la logique pour trouver le prix du plat le plus cher avec Linq
            // Utilisez Max
            decimal prixLinq = _context.Plat.Max(p => p.Prix);

            return new DecimalViewModel("Quel est le prix du plat le plus cher?", prix, prixLinq);
        }

        private DecimalViewModel ValeurTotalDesPlats()
        {
            // TODO: Calculer la valeur totale des plats avec boucle et Linq
            var liste = _context.Plat.ToList();
            decimal total = 0;
            foreach (var plat in liste)
            {
                total += plat.Prix;
            }
            // Utilisez Sum avec Linq
            decimal totalLinq = _context.Plat.Sum(p => p.Prix);
            return new DecimalViewModel("Quelle est la valeur totale des plats?", totalLinq, total);
        }

        private DecimalViewModel ValeurTotalDesCommandes(string nomClient)
        {
            // TODO: Calculer la valeur totale des commandes du client [nomClient] avec boucle et Linq
            decimal valeurTotal = 0;
            foreach (var commandes in _context.Commande)
            {
                if (commandes.Client.Nom == nomClient)
                {
                    foreach (var plats in commandes.CommandesPlats)
                    {
                        valeurTotal = (plats.Plat.Prix * plats.Quantite);
                    }
                }
            }
            
            // Linq: Utilisez Where et 2 fois Sum
            var listeLinq = _context.Commande.ToList();
            // Attention: c'est plus facile si vous faites un ToList() et faites le linq sur la liste et non pas le DbSet
            // on en parlera au prochain cours
            // Faites votre requête Linq sur listeLinq
            decimal valeurTotalLinq = listeLinq.Where(c => c.Client.Nom == nomClient).Sum(c => c.CommandesPlats.Sum(cp => cp.Plat.Prix));

            return new DecimalViewModel("Quelle est la valeur totale des commandes de " + nomClient + "?", valeurTotalLinq, valeurTotal);
        }

        private DecimalViewModel PrixCommandeLaPlusCher()
        {
            // TODO: Trouver le côut total de la commande la plus chère
            
            // Linq: Utilisez Sum et Max
            var listeLinq = _context.Commande.ToList();
            // Attention: c'est plus facile si vous faites un ToList() et faites le linq sur la liste et non pas le DbSet
            // on en parlera au prochain cours
            // Faites votre requête Linq sur listeLinq

            return new DecimalViewModel("Quel est le prix de la commande la plus chère?", 0, 0);
        }

        private VegetarienViewModel Vegetarien(string nomDuResto)
        {
            // TODO: Est-ce que le restaurant avec le nom [nomDuRest] a au moins un plat végé?
            bool? optionVege = null;
            // TODO: Est-ce que le restaurant a UNIQUEMENT des plats végés?
            bool? toutVege = null;

            // TODO: Même chose, mais avec Linq
            // Utilisez Where, All et Any
            bool? optionVegeLinq = null;
            bool? toutVegeLinq = null;

            return new VegetarienViewModel("Status végétarien du restaurant : " + nomDuResto, toutVege, toutVegeLinq, optionVege, optionVegeLinq);
        }

        // Méthode pratique pour utiliser List<>.Sort()
        private int ComparerPrix(Plat platA, Plat platB)
        {
            decimal diff = platA.Prix - platB.Prix;
            if (diff > 0)
                return 1;
            if(diff < 0)
                return -1;
            return 0;
        }

        private PlatsViewModel PlatsVegeOrdeCroissantDePrix()
        {
            // Remplir une liste avec les plats végés en ordre croissant de prix
            // Note: Il y a une méthode ComparerPrix qui est déjà fournie au dessus
            // Remplir la liste avec une boucle
            List<Plat> plats = new List<Plat>();
            // Obtenir la liste avec Linq
            // Utilisez Where, OrderBy et ToList
            List<Plat> platsLinq = new List<Plat>();

            return new PlatsViewModel("Quels sont les plats végétariens?", plats, platsLinq);
        }

        private PlatsViewModel PlatsLesPlusChersOrdeDecroissantDePrix(int nbPlats)
        {
            // Remplir une liste avec les plats les plus chers en ordre décroissant
            // La liste doit avoir uniquement [nbPlats] entrées
            // Utilisez OrderByDescending, Take et ToList
            List<Plat> platsLesPlusChers = new List<Plat>();
            List<Plat> platsLinq = new List<Plat>();
            
            return new PlatsViewModel("Quels sont les plats les plus chers?", platsLesPlusChers, platsLinq);
        }

    }
}
