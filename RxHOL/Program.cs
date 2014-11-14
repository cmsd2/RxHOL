using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Threading.Tasks;
using System.Windows.Forms;
using RxHOL.Extensions;
using RxHOL.DictServiceRef;
using System.Threading.Tasks;

namespace RxHOL
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var svc = new DictServiceSoapClient("DictServiceSoap");

            Func<string, string, string, IObservable<DictionaryWord[]>> matchInDict = (dict, term, mode) =>
            {
                return Task.Factory.FromAsync<string, string, string, DictionaryWord[]>(
                    svc.BeginMatchInDict, svc.EndMatchInDict, dict, term, mode, null
                    ).ToObservable();
            };

            Func<string, IObservable<DictionaryWord[]>> matchInWordNetByPrefix = term => matchInDict("wn", term, "prefix");

            var txt = new TextBox();
            var lst = new ListBox { Top = txt.Height + 10 };
            var frm = new Form
            {
                Controls = { txt, lst }
            };

            var input = (from evt in Observable.FromEventPattern(txt, "TextChanged")
                         select ((TextBox)evt.Sender).Text)
                             .Where(term => term.Length >= 3)
                             .Throttle(TimeSpan.FromSeconds(1))
                             .DistinctUntilChanged()
                             .Do(x => Console.WriteLine(x));

            var res = from term in input
                      from words in matchInWordNetByPrefix(term)
                      .Finally(() => Console.WriteLine("Disposed of request for " + term))
                      .TakeUntil(input)
                      select words;

            
            using (res.ObserveOn(frm).Subscribe((words) =>
            {
                lst.Items.Clear();
                var wordsArray = (from word in words select word.Word).ToArray();
                lst.Items.AddRange(wordsArray);
            }, (ex) =>
            {
                MessageBox.Show("An error occurred: " + ex.Message, frm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }))
            {
                Application.Run(frm);
            }
        }
    }
}
