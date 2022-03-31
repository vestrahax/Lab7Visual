using Avalonia.Media;
using HW_7.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HW_7.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase content;
        ObservableCollection<Student> Items { get; set; }
        ObservableCollection<float?> AverageGrades { get; set; }
        ObservableCollection<IBrush> AverageGradesBrushes { get; set; }
        public ViewModelBase Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public void AddNewStudent()
        {
            Items.Insert(0, new Student("NEW STUDENT"));
        }

        public void RemoveCheckedStudents()
        {
            var neededStudents = this.Items.Where(x => !x.isChecked).ToList();
            this.Items.Clear();
            foreach (var neededStudent in neededStudents)
            {
                this.Items.Add(neededStudent);
            }

        }

        public MainWindowViewModel()
        {
            this.Items = new ObservableCollection<Student>();
            this.AverageGrades = new ObservableCollection<float?>() { 0, 0, 0 };
            this.AverageGradesBrushes = new ObservableCollection<IBrush>() { new SolidColorBrush(Brushes.White.Color), new SolidColorBrush(Brushes.White.Color), new SolidColorBrush(Brushes.White.Color) };
            this.Items.CollectionChanged += MyItemsSource_CollectionChanged;
            this.Content = new MainViewModel();
        }

        public void WriteToBinaryFile(string filePath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Student>));

            using (StreamWriter wr = new StreamWriter(filePath))
            {
                xs.Serialize(wr, this.Items);
            }
        }

        public void ReadFromBinaryFile(string filePath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Student>));
            using (StreamReader sr = new StreamReader(filePath))
            {
                this.Items.Clear();
                try
                {
                    this.Items = (ObservableCollection<Student>)xs.Deserialize(sr);
                    foreach (Student s in this.Items)
                    {
                        var gradeList = new List<ControlMark>(3);
                        gradeList.Add(s.ControlMarks[3]);
                        gradeList.Add(s.ControlMarks[4]);
                        gradeList.Add(s.ControlMarks[5]);
                        s.ControlMarks.Clear();
                        foreach (ControlMark mark in gradeList)
                        {
                            s.ControlMarks.Add(mark);
                        }

                        s.CalculateAverage();
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        public void OpenFileView()
        {
            this.Content = new FileViewModel();
        }

        public void OpenMainView()
        {
            this.Content = new MainViewModel();
        }

        public void CalculateAveragesOfStudents()
        {

            for (int i = 0; i < 3; i++)
            {
                AverageGrades[i] = 0;
            }
            foreach (Student s in this.Items)
            {
                for (int i = 0; i < 3; i++)
                {
                    AverageGrades[i] += s.ControlMarks[i].Mark;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                AverageGrades[i] /= this.Items.Count;
                if (AverageGrades[i] is not null)
                {
                    if (AverageGrades[i] < 1.5)
                    {
                        this.AverageGradesBrushes[i] = new SolidColorBrush(Brushes.Yellow.Color);
                    }
                    if (AverageGrades[i] < 1)
                    {
                        this.AverageGradesBrushes[i] = new SolidColorBrush(Brushes.Red.Color);
                    }
                    if (AverageGrades[i] >= 1.5)
                    {
                        this.AverageGradesBrushes[i] = new SolidColorBrush(Brushes.LightGreen.Color);
                    }
                }
                else
                {
                    this.AverageGradesBrushes[i] = new SolidColorBrush(Brushes.White.Color);
                }
            }
        }

        void MyItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Student item in e.NewItems)
                    item.PropertyChanged += MyType_PropertyChanged;

            if (e.OldItems != null)
                foreach (Student item in e.OldItems)
                    item.PropertyChanged -= MyType_PropertyChanged;
        }

        void MyType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CalculateAveragesOfStudents();
        }

    }
}
