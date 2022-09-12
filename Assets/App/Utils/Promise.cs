using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
	public partial class Promise
	{
        #region Events & Actions

        private static Func<object, object> Wrap1(Action action) { if (action == null) return null; return x => { action(); return null; }; }
		private static Func<object, object> Wrap2<I>(Action<I> action) { if (action == null) return null; return x => { action((I)x); return null; }; }
		private static Func<object, object> Wrap3<O>(Func<O> func) { if (func == null) return null; return x => func(); }
		private static Func<object, object> Wrap4<I, O>(Func<I, O> func) { if (func == null) return null; return x => (object)func((I)x); }

		private Func<object, object> onFulfilled;
		private Func<object, object> onRejected;

		#endregion

		#region Constants

		private const string PENDING = "PENDING";
		private const string FULFILLED = "FULFILLED";
		private const string REJECTED = "REJECTED";

        #endregion

        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Protected fields
        // ---------------------------------------------------------------------------------------------------------

        protected object value = null;
		private string state = PENDING;

        // ---------------------------------------------------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------------------------------------------------
        private Queue<Promise> reacts = new Queue<Promise>();

        #endregion

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public Promise Then(Action f, Action<Exception> r = null) { return Th(Wrap1(f), Wrap2(r)); }
		public Promise Then<I>(Action<I> f, Action<Exception> r = null) { return Th(Wrap2(f), Wrap2(r)); }
		public Promise Then<O>(Func<O> f, Func<Exception, O> r = null) { return Th(Wrap3(f), Wrap4(r)); }
		public Promise Then<I, O>(Func<I, O> f, Func<Exception, O> r = null) { return Th(Wrap4(f), Wrap4(r)); }

		public Promise Catch(Action<Exception> r = null) { return Th(null, Wrap2(r)); }
		public Promise Catch<O>(Func<Exception, O> r = null) { return Th(null, Wrap4(r)); }

        public void Fulfill() { Fulfill(null); }
        public void Fulfill(object value) { resolveProcedure(value, FULFILLED); }

        public void Reject(Exception value) { resolveProcedure(value ?? new Exception("default exception"), REJECTED); }

        // ---------------------------------------------------------------------------------------------------------
        // Private Methods
        // ---------------------------------------------------------------------------------------------------------

        private Promise Th(Func<object, object> f, Func<object, object> r)
		{
			var promise = new Promise
			{
				onFulfilled = f ?? (x => x),
				onRejected = r ?? (x => throw (Exception)x)
			};

			if (state != PENDING) promise.reaction(value, state);
			else reacts.Enqueue(promise);

			return promise;
		}
		

		private void resolveProcedure(object value, string state)
		{
			if (value == this)
				resolve(new Exception(), REJECTED);

			else if (value is Promise)
				((Promise)value).Then<object>(o => Fulfill(o), Reject);

			else
				resolve(value, state);
		}


		private void resolve(object value, string state)
		{
			if (this.state == PENDING)
			{
				this.state = state;
				this.value = value;

				while (reacts.Count > 0)
					reacts.Dequeue().reaction(value, state);
			}
		}


		private void reaction(object value, string state)
		{
			try
			{
				Fulfill(state == FULFILLED ? onFulfilled(value) : onRejected(value));
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				Reject(e);
			}
		}

        #endregion
    }

    public partial class Promise : CustomYieldInstruction
	{
		public override bool keepWaiting
		{
			get
			{
				return state == PENDING;
			}
		}
	}

	public partial class Promise
	{
		public static Promise Resolved(object value = null)
		{
			var promise = new Promise();
			promise.Fulfill(value);
			return promise;
		}


		public static Promise Rejected(Exception value = null)
		{
			var promise = new Promise();
			promise.Reject(value);
			return promise;
		}


		public static Promise All(params Promise[] args)
		{
			if (args == null || args.Length == 0)
				return Promise.Resolved();

			var result = new Promise();
			var numAwait = args.Length;

			Action fulfill = () =>
			{
				if (--numAwait == 0) result.Fulfill();
			};

			foreach (var arg in args)
				arg.Then(fulfill, result.Reject);

			return result;
		}
	}

	//
	// A little bit of experimentation - don't use this class
	//

	public class Promise<T> : Promise
	{
		public T Value
		{
			get { return (T)value; }
		}

		public Exception Exception
		{
			get { return value as Exception; }
		}
	}
}
