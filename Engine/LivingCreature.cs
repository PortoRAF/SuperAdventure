using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	public class LivingCreature : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _currentHitPoints;
		public int CurrentHitPoints
		{
			get { return _currentHitPoints; }
			set
			{
				_currentHitPoints = value;
				OnPropertyChanged("CurrentHitPoints"); // calls function when CurrentHitPoints is set
			}
		}
		public int MaximumHitPoints { get; set; }
		
		public LivingCreature(int currentHitPoints, int maximumHitPoints)
		{
			CurrentHitPoints = currentHitPoints;
			MaximumHitPoints = maximumHitPoints;
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
