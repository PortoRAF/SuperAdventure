using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace SuperAdventure
{
	public partial class SuperAdventure : Form
	{
		// Create player object on class scope
		private Player _player;
		
		public SuperAdventure()
		{
			InitializeComponent();

			// Instantiate player object and initialize its properties
			_player = new Player();
			_player.CurrentHitPoints = 10;
			_player.MaximumHitPoints = 10;
			_player.Gold = 20;
			_player.ExperiencePoints = 0;
			_player.Level = 1;

			// Assign labels text values to match player's
			lblHitPoints.Text	= _player.CurrentHitPoints.ToString();
			lblGold.Text		= _player.Gold.ToString();
			lblExperience.Text	= _player.ExperiencePoints.ToString();
			lblLevel.Text		= _player.Level.ToString();
		}
	}
}
