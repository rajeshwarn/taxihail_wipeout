using System;
using System.IO;
using System.Linq;

public class CodeGen
{
	public static void Main ()
	{
		Console.WriteLine (@"
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
	public static partial class Observable
	{
		");

		for (int i = 1; i <= 14; i++) {
			string s = String.Join (", ", (from t in Enumerable.Range (1, i) select "TArg" + t).ToArray ());
			string s2 = String.Join (", ", (from t in Enumerable.Range (1, i) select "t" + t).ToArray ());
			Console.WriteLine (@"
		public static Func<{0}, IObservable<Unit>> FromAsyncPattern<{0}> (Func<{0}, AsyncCallback, object, IAsyncResult> begin, Action<IAsyncResult> end)
		{{
			return ({1}) => {{
				var sub = new Subject<Unit> ();
				begin ({1}, (res) => {{
				try {{
					end (res);
					sub.OnNext (Unit.Default);
					sub.OnCompleted ();
				}} catch (Exception ex) {{
					sub.OnError (ex);
				}}
				}}, sub); return sub; }};
		}}
		
		public static Func<{0}, IObservable<TResult>> FromAsyncPattern<{0}, TResult> (Func<{0}, AsyncCallback, Object, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
		{{
			return ({1}) => {{
				var sub = new Subject<TResult> ();
				begin ({1}, (res) => {{
				try {{
					var result = end (res);
					sub.OnNext (result);
					sub.OnCompleted ();
				}} catch (Exception ex) {{
					sub.OnError (ex);
				}}
				}}, sub); return sub; }};
		}}
		", s, s2);
		}

		for (int i = 2; i <= 16; i++) {
			string s = String.Join (", ", (from t in Enumerable.Range (1, i) select "TArg" + t).ToArray ());
			string s2 = String.Join (", ", (from t in Enumerable.Range (1, i) select "t" + t).ToArray ());
			
			Console.WriteLine (@"
		public static Func<{0}, IObservable<Unit>> ToAsync<{0}> (this Action<{0}> action)
		{{
			return ({1}) => Start (() => action ({1}));
		}}
		
		public static Func<{0}, IObservable<Unit>> ToAsync<{0}> (this Action<{0}> action, IScheduler scheduler)
		{{
			return ({1}) => Start (() => action ({1}), scheduler);
		}}
		
		public static Func<{0}, IObservable<TResult>> ToAsync<{0}, TResult> (this Func<{0}, TResult> function)
		{{
			return ({1}) => Start (() => function ({1}));
		}}
		
		public static Func<{0}, IObservable<TResult>> ToAsync<{0}, TResult> (this Func<{0}, TResult> function, IScheduler scheduler)
		{{
			return ({1}) => Start (() => function ({1}), scheduler);
		}}
		", s, s2);
		}

		Console.WriteLine ("#if REACTIVE_2_0");

		for (int i = 3; i <= 16; i++) {
			string s = String.Join (", ", (from t in Enumerable.Range (1, i) select "TSource" + t).ToArray ());
			string s2 = String.Join (", ", (from t in Enumerable.Range (1, i) select "IObservable<TSource" + t + "> source" + t).ToArray ());
			string s3 = String.Join (", ", (from t in Enumerable.Range (1, i) select "TSource" + t).ToArray ());
			string s4 = String.Join ("\n\t\t\t", (from t in Enumerable.Range (1, i) select "if (source" + t + " == null) throw new ArgumentNullException (\"source" + t + "\");").ToArray ());
			string s5 = String.Join (").And (", (from t in Enumerable.Range (1, i) select "source" + t).ToArray ());
			Console.WriteLine (@"
		public static IObservable<TResult> Zip<{0},TResult> (this {1}, Func<{0},TResult> resultSelector)
		{{
			{3}
			return When (({4}).Then (resultSelector));
		}}", s, s2, s3, s4, s5);
		}

		Console.WriteLine ("#endif");

		Console.WriteLine (@"
	}
}
");
	}
}
