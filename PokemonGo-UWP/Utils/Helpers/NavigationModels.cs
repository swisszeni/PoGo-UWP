using PokemonGo_UWP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
    public class PokemonDetailPageNavModel
    {
        public string SelectedPokemonId { get; set; }
        public PokemonSortingModes SortingMode { get; set; }
        public Views.PokemonDetailPageViewMode ViewMode { get; set; }
    }


    public class PokemonSelectionPageNavModel
    {
        public Func<PokemonDataWrapper, bool> DisplayingFilter { get; set; }
        public Func<PokemonDataWrapper, bool> SelectableFilter { get; set; }
        public bool AutoDismissOnSelection { get; set; }
        public DelegateCommand<PokemonDataWrapper> OnPokemonSelectedCommand { get; set; }
    }
}
