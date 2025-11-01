using Avalonia.Media;
using SportCentre1.Data;
using System;

namespace SportCentre1.Models
{
    public class EquipmentViewModel
    {
        private readonly Equipment _equipment;

        public EquipmentViewModel(Equipment equipment)
        {
            _equipment = equipment;
        }

        public int Equipmentid => _equipment.Equipmentid;
        public string Name => _equipment.Name ?? "";
        public string Status => _equipment.Status ?? "";
        public int? Quantity => _equipment.Quantity;
        public DateOnly? Lastmaintenancedate => _equipment.Lastmaintenancedate;

        public bool IsMaintenanceOverdue
        {
            get
            {
                if (!_equipment.Lastmaintenancedate.HasValue) return false;

                return (DateOnly.FromDateTime(DateTime.Now).DayNumber - _equipment.Lastmaintenancedate.Value.DayNumber) > 180;
            }
        }

        public IBrush RowColor => IsMaintenanceOverdue ? Brushes.LightCoral : Brushes.Transparent;
    }
}