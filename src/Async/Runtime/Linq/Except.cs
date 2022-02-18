﻿using System;
using System.Collections.Generic;
using System.Threading;
using Appalachia.Utility.Async.Internal;

namespace Appalachia.Utility.Async.Linq
{
    public static partial class AppaTaskAsyncEnumerable
    {
        public static IAppaTaskAsyncEnumerable<TSource> Except<TSource>(
            this IAppaTaskAsyncEnumerable<TSource> first,
            IAppaTaskAsyncEnumerable<TSource> second)
        {
            Error.ThrowArgumentNullException(first,  nameof(first));
            Error.ThrowArgumentNullException(second, nameof(second));

            return new Except<TSource>(first, second, EqualityComparer<TSource>.Default);
        }

        public static IAppaTaskAsyncEnumerable<TSource> Except<TSource>(
            this IAppaTaskAsyncEnumerable<TSource> first,
            IAppaTaskAsyncEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            Error.ThrowArgumentNullException(first,    nameof(first));
            Error.ThrowArgumentNullException(second,   nameof(second));
            Error.ThrowArgumentNullException(comparer, nameof(comparer));

            return new Except<TSource>(first, second, comparer);
        }
    }

    internal sealed class Except<TSource> : IAppaTaskAsyncEnumerable<TSource>
    {
        public Except(
            IAppaTaskAsyncEnumerable<TSource> first,
            IAppaTaskAsyncEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            this.first = first;
            this.second = second;
            this.comparer = comparer;
        }

        #region Fields and Autoproperties

        private readonly IAppaTaskAsyncEnumerable<TSource> first;
        private readonly IAppaTaskAsyncEnumerable<TSource> second;
        private readonly IEqualityComparer<TSource> comparer;

        #endregion

        #region IAppaTaskAsyncEnumerable<TSource> Members

        public IAppaTaskAsyncEnumerator<TSource> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return new _Except(first, second, comparer, cancellationToken);
        }

        #endregion

        #region Nested type: _Except

        private class _Except : AsyncEnumeratorBase<TSource, TSource>
        {
            public _Except(
                IAppaTaskAsyncEnumerable<TSource> first,
                IAppaTaskAsyncEnumerable<TSource> second,
                IEqualityComparer<TSource> comparer,
                CancellationToken cancellationToken) : base(first, cancellationToken)
            {
                this.second = second;
                this.comparer = comparer;
            }

            #region Static Fields and Autoproperties

            private static Action<object> HashSetAsyncCoreDelegate = HashSetAsyncCore;

            #endregion

            #region Fields and Autoproperties

            private readonly IAppaTaskAsyncEnumerable<TSource> second;

            private readonly IEqualityComparer<TSource> comparer;
            private AppaTask<HashSet<TSource>>.Awaiter awaiter;

            private HashSet<TSource> set;

            #endregion

            /// <inheritdoc />
            protected override bool OnFirstIteration()
            {
                if (set != null)
                {
                    return false;
                }

                awaiter = second.ToHashSetAsync(cancellationToken).GetAwaiter();
                if (awaiter.IsCompleted)
                {
                    set = awaiter.GetResult();
                    SourceMoveNext();
                }
                else
                {
                    awaiter.SourceOnCompleted(HashSetAsyncCoreDelegate, this);
                }

                return true;
            }

            /// <inheritdoc />
            protected override bool TryMoveNextCore(bool sourceHasCurrent, out bool result)
            {
                if (sourceHasCurrent)
                {
                    var v = SourceCurrent;
                    if (set.Add(v))
                    {
                        Current = v;
                        result = true;
                        return true;
                    }

                    result = default;
                    return false;
                }

                result = false;
                return true;
            }

            private static void HashSetAsyncCore(object state)
            {
                var self = (_Except)state;

                if (self.TryGetResult(self.awaiter, out var result))
                {
                    self.set = result;
                    self.SourceMoveNext();
                }
            }
        }

        #endregion
    }
}
