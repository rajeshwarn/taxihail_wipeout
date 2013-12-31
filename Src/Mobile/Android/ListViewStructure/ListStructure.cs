using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.ListViewStructure
{
    public class ListStructure
    {
        private readonly List<Section> _sections;

        public ListStructure(float defaultRowHeight, bool useSectionIndex)
        {
            UseSectionIndex = useSectionIndex;
            _sections = new List<Section>();
            RowHeight = defaultRowHeight;
        }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool UseSectionIndex { get; set; }

        public IEnumerable<Section> Sections
        {
            get { return _sections; }
        }


        public float RowHeight { get; set; }

        public IEnumerable<SectionItem> AllItems
        {
            get { return Sections.SelectMany(section => section.Items); }
        }


        public int ItemsCount
        {
            get { return Sections.Sum(s => s.Items.Count()); }
        }

        public int SectionsCount
        {
            get { return Sections.Count(); }
        }

        public int TableViewHeight
        {
            get { return Sections.Sum(s => s.Items.Sum(i => (int) i.RowHeight)); }
        }

        public Section AddSection()
        {
            var section = new Section(this);
            _sections.Add(section);
            return section;
        }

        public Section AddSection(string sectionLabel)
        {
            var section = AddSection();
            section.SectionLabel = sectionLabel;
            return section;
        }
    }
}