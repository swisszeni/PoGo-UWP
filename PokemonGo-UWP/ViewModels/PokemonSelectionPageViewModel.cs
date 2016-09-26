using POGOProtos.Data;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonSelectionPageViewModel : ViewModelBase
    {

        //public class PokemonSelectionPageNavModel
        //{
        //    public Func<PokemonDataWrapper, bool> DisplayingFilter { get; set; }
        //    public Func<PokemonDataWrapper, bool> SelectableFilter { get; set; }
        //    public bool AutoDismissOnSelection { get; set; }
        //    public DelegateCommand<PokemonDataWrapper> OnPokemonSelectedCommand { get; set; }
        //}


        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                PokemonInventory = JsonConvert.DeserializeObject<ObservableCollection<PokemonDataWrapper>>((string)suspensionState[nameof(PokemonInventory)]);
                EggsInventory = JsonConvert.DeserializeObject<ObservableCollection<PokemonDataWrapper>>((string)suspensionState[nameof(EggsInventory)]);
                CurrentPokemonSortingMode = (PokemonSortingModes)suspensionState[nameof(CurrentPokemonSortingMode)];
                PlayerProfile = GameClient.PlayerProfile;
            }
            else
            {
                // Navigating from inventory page so we need to load the pokemoninventory and the current pokemon
                var navParam = (PokemonSelectionPageNavModel)parameter;
                navParam.
                Load(Convert.ToUInt64(navParam.SelectedPokemonId), navParam.SortingMode, navParam.ViewMode);



                // Navigating from game page, so we need to actually load the inventory
                // The sorting mode is directly bound to the settings
                PokemonInventory = new ObservableCollection<PokemonDataWrapper>(GameClient.PokemonsInventory
                    .Select(pokemonData => new PokemonDataWrapper(pokemonData))
                    .SortBySortingmode(CurrentPokemonSortingMode));

                RaisePropertyChanged(() => PokemonInventory);
                RaisePropertyChanged(() => TotalPokemonCount);

                PlayerProfile = GameClient.PlayerProfile;
            }

            await Task.CompletedTask;
        }

        public void Load(ulong selectedPokemonId, PokemonSortingModes sortingMode, PokemonDetailPageViewMode viewMode)
        {
            PokemonInventory.Clear();
            SortingMode = sortingMode;
            ViewMode = viewMode;
            if (viewMode == PokemonDetailPageViewMode.Normal)
            {
                // Navigating from inventory page so we need to load the pokemoninventory and the current pokemon
                PokemonInventory.AddRange(GameClient.PokemonsInventory.Select(pokemonData => new PokemonDataWrapper(pokemonData)).SortBySortingmode(sortingMode));
                SelectedPokemon = PokemonInventory.FirstOrDefault(pokemon => pokemon.Id == selectedPokemonId);
            }
            else
            {
                // Navigating from Capture, Egg hatch or evolve, only show this pokemon
                PokemonInventory.Add(GameClient.PokemonsInventory.Where(pokemon => pokemon.Id == selectedPokemonId).Select(pokemonData => new PokemonDataWrapper(pokemonData)).FirstOrDefault());
                SelectedPokemon = PokemonInventory.First();
            }

            StardustAmount = GameClient.PlayerProfile.Currencies.FirstOrDefault(item => item.Name.Equals("STARDUST")).Amount;
            PlayerTeamIsSet = GameClient.PlayerProfile.Team != TeamColor.Neutral;
        }

        /// <summary>
        /// Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(PokemonInventory)] = JsonConvert.SerializeObject(PokemonInventory);
                suspensionState[nameof(CurrentPokemonSortingMode)] = CurrentPokemonSortingMode;
            }
            await Task.CompletedTask;
        }

        #endregion

        #region Bindable Game Vars

        /// <summary>
        /// Player's profile, we use it just for the maximum ammount of pokemon
        /// </summary>
        private PlayerData _playerProfile;
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        /// Sorting mode for current Pokemon view
        /// </summary>
        public PokemonSortingModes CurrentPokemonSortingMode
        {
            get { return SettingsService.Instance.PokemonSortingMode; }
            set
            {
                SettingsService.Instance.PokemonSortingMode = value;
                RaisePropertyChanged(() => CurrentPokemonSortingMode);

                // When this changes we need to sort the collection again
                UpdateSorting();
            }
        }

        /// <summary>
        /// Reference to Pokemon inventory
        /// </summary>
        public ObservableCollection<PokemonDataWrapper> PokemonInventory { get; private set; } =
            new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        /// Total amount of Pokemon in players inventory
        /// </summary>
        public int TotalPokemonCount
        {
            get { return PokemonInventory.Count + GameClient.EggsInventory.Count; }
        }

        #endregion

        #region GameLogic

        #region Pokemon Inventory Handling

        /// <summary>
        /// Sort the PokemonInventory with the CurrentPokemonSortingMode 
        /// </summary>
        private void UpdateSorting()
        {
            PokemonInventory =
                new ObservableCollection<PokemonDataWrapper>(PokemonInventory.SortBySortingmode(CurrentPokemonSortingMode));

            RaisePropertyChanged(() => PokemonInventory);
        }

        #endregion

        #endregion
    }
}
