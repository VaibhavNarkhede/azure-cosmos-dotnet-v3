﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace Microsoft.Azure.Cosmos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Routing;

    /// <summary>
    /// FeedRange that represents an effective partition key range.
    /// </summary>
    internal sealed class FeedRangeEPK : FeedRangeInternal
    {
        public static readonly FeedRangeEPK FullRange = new FeedRangeEPK(new Documents.Routing.Range<string>(
            Documents.Routing.PartitionKeyInternal.MinimumInclusiveEffectivePartitionKey,
            Documents.Routing.PartitionKeyInternal.MaximumExclusiveEffectivePartitionKey,
            isMinInclusive: true,
            isMaxInclusive: false));

        public FeedRangeEPK(Documents.Routing.Range<string> range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            this.Range = range;
        }

        public Documents.Routing.Range<string> Range { get; }

        public override Task<List<Documents.Routing.Range<string>>> GetEffectiveRangesAsync(
            IRoutingMapProvider routingMapProvider,
            string containerRid,
            Documents.PartitionKeyDefinition partitionKeyDefinition) => Task.FromResult(new List<Documents.Routing.Range<string>>() { this.Range });

        public override async Task<IEnumerable<string>> GetPartitionKeyRangesAsync(
            IRoutingMapProvider routingMapProvider,
            string containerRid,
            Documents.PartitionKeyDefinition partitionKeyDefinition,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<Documents.PartitionKeyRange> partitionKeyRanges = await routingMapProvider.TryGetOverlappingRangesAsync(containerRid, this.Range, forceRefresh: false);
            return partitionKeyRanges.Select(partitionKeyRange => partitionKeyRange.Id);
        }

        public override void Accept(IFeedRangeVisitor visitor) => visitor.Visit(this);

        public override Task<TResult> AcceptAsync<TResult>(
            IFeedRangeAsyncVisitor<TResult> visitor,
            CancellationToken cancellationToken = default) => visitor.VisitAsync(this, cancellationToken);

        public override string ToString() => this.Range.ToString();
    }
}