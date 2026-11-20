using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace YS.Knife.Extensions.UnitTest
{
    public class CollectionExtensionsTest
    {
        #region AppendTo (Func overload)

        [Fact]
        public void AppendTo_ShouldAddNewItemsAndKeepExisting()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" }
            };

            source.AppendTo(target, s => s.Id, t => t.Id, s => new TargetItem { Id = s.Id, Name = s.Name });

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            // existing item should NOT be modified
            target.Single(t => t.Id == 1).Name.Should().Be("OldA");
            target.Single(t => t.Id == 2).Name.Should().Be("B");
        }

        [Fact]
        public void AppendTo_ShouldInvokeOnAddNewForNewItems()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" }
            };
            var addedIds = new List<int>();

            source.AppendTo(target, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                t => addedIds.Add(t.Id));

            addedIds.Should().BeEquivalentTo(new[] { 2 });
        }

        [Fact]
        public void AppendTo_WithEmptySource_ShouldNotChangeTarget()
        {
            var source = new List<SourceItem>();
            var target = new List<TargetItem> { new() { Id = 1, Name = "A" } };

            source.AppendTo(target, s => s.Id, t => t.Id, s => new TargetItem { Id = s.Id, Name = s.Name });

            target.Should().HaveCount(1);
            target.Single().Id.Should().Be(1);
        }

        [Fact]
        public void AppendTo_WithAllExistingKeys_ShouldNotAddAnything()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem> { new() { Id = 1, Name = "OldA" } };

            source.AppendTo(target, s => s.Id, t => t.Id, s => new TargetItem { Id = s.Id, Name = s.Name });

            target.Should().HaveCount(1);
            target.Single().Name.Should().Be("OldA");
        }

        #endregion

        #region AppendTo (Expression overload)

        [Fact]
        public void AppendTo_WithExpression_ShouldAddNewItems()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" }
            };

            source.AppendTo(target, s => s.Id, s => new TargetItem { Id = s.Id, Name = s.Name });

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            target.Single(t => t.Id == 2).Name.Should().Be("B");
        }

        [Fact]
        public void AppendTo_WithExpression_ShouldInvokeOnAddNew()
        {
            var source = new List<SourceItem> { new() { Id = 3, Name = "C" } };
            var target = new List<TargetItem> { new() { Id = 1, Name = "A" } };
            var added = new List<TargetItem>();

            source.AppendTo(target, s => s.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                t => added.Add(t));

            added.Should().HaveCount(1);
            added.Single().Id.Should().Be(3);
        }

        #endregion

        #region MergeTo (Func overload)

        [Fact]
        public void MergeTo_ShouldAddNewAndUpdateExisting_NotRemove()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "NewA" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" },
                new() { Id = 3, Name = "C" } // not in source, should be kept
            };

            source.MergeTo(target, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name);

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            target.Single(t => t.Id == 1).Name.Should().Be("NewA"); // updated
            target.Single(t => t.Id == 2).Name.Should().Be("B");    // added
            target.Single(t => t.Id == 3).Name.Should().Be("C");    // kept
        }

        [Fact]
        public void MergeTo_ShouldInvokeUpdateActionForExisting()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "NewA" } };
            var target = new List<TargetItem> { new() { Id = 1, Name = "OldA" } };
            var updatedIds = new List<int>();

            source.MergeTo(target, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => { t.Name = s.Name; updatedIds.Add(t.Id); });

            updatedIds.Should().BeEquivalentTo(new[] { 1 });
        }

        #endregion

        #region MergeTo (Expression overload)

        [Fact]
        public void MergeTo_WithExpression_ShouldAddAndUpdate()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "NewA" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem> { new() { Id = 1, Name = "OldA" } };

            source.MergeTo(target, s => s.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name);

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            target.Single(t => t.Id == 1).Name.Should().Be("NewA");
            target.Single(t => t.Id == 2).Name.Should().Be("B");
        }

        #endregion

        #region UpdateTo (Func overload)

        [Fact]
        public void UpdateTo_ShouldAddUpdateAndRemove()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "NewA" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" },
                new() { Id = 3, Name = "C" } // not in source, should be removed
            };

            source.UpdateTo(target, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name);

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            target.Single(t => t.Id == 1).Name.Should().Be("NewA");
            target.Single(t => t.Id == 2).Name.Should().Be("B");
        }

        [Fact]
        public void UpdateTo_ShouldInvokeOnRemoveOldForRemovedItems()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" },
                new() { Id = 2, Name = "B" },
                new() { Id = 3, Name = "C" }
            };
            var removedIds = new List<int>();

            source.UpdateTo(target, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name,
                onRemoveOld: t => removedIds.Add(t.Id));

            removedIds.Should().BeEquivalentTo(new[] { 2, 3 });
        }

        [Fact]
        public void UpdateTo_ShouldInvokeOnAddNewForNewItems()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem> { new() { Id = 1, Name = "OldA" } };
            var addedIds = new List<int>();

            source.UpdateTo(target, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name,
                onAddNew: t => addedIds.Add(t.Id));

            addedIds.Should().BeEquivalentTo(new[] { 2 });
        }

        #endregion

        #region UpdateTo (Expression overload)

        [Fact]
        public void UpdateTo_WithExpression_ShouldAddUpdateAndRemove()
        {
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "NewA" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" },
                new() { Id = 3, Name = "C" }
            };

            source.UpdateTo(target, s => s.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name);

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            target.Single(t => t.Id == 1).Name.Should().Be("NewA");
        }

        #endregion

        #region SaveTo direct (Func overload)

        [Theory]
        [InlineData(CollectionSaveMode.Append)]
        [InlineData(CollectionSaveMode.Merge)]
        [InlineData(CollectionSaveMode.Update)]
        public void SaveTo_EmptySource_ShouldNotAdd(CollectionSaveMode mode)
        {
            var source = new List<SourceItem>();
            var target = new List<TargetItem> { new() { Id = 1, Name = "A" } };

            source.SaveTo(target, mode, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name);

            if (mode == CollectionSaveMode.Update)
            {
                target.Should().BeEmpty(); // removed because not in source
            }
            else
            {
                target.Should().HaveCount(1);
            }
        }

        [Fact]
        public void SaveTo_AppendMode_ShouldNotRemoveOrUpdate()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "NewA" } };
            var target = new List<TargetItem>
            {
                new() { Id = 1, Name = "OldA" },
                new() { Id = 2, Name = "B" }
            };
            var updateCalled = false;

            source.SaveTo(target, CollectionSaveMode.Append, s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => updateCalled = true);

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            target.Single(t => t.Id == 1).Name.Should().Be("OldA");
            updateCalled.Should().BeFalse();
        }

        #endregion

        #region Null argument checks

        [Fact]
        public void SaveTo_NullSource_ShouldThrow()
        {
            ICollection<SourceItem> source = null!;
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("source");
        }

        [Fact]
        public void SaveTo_NullTarget_ShouldThrow()
        {
            var source = new List<SourceItem>();
            ICollection<TargetItem> target = null!;

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("target");
        }

        [Fact]
        public void SaveTo_NullSourceKey_ShouldThrow()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                null, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("sourceKey");
        }

        [Fact]
        public void SaveTo_NullTargetKey_ShouldThrow()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                s => s.Id, null,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("targetKey");
        }

        [Fact]
        public void SaveTo_NullConvertFunc_ShouldThrow()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                s => s.Id, t => t.Id,
                null, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("convertFunc");
        }

        [Fact]
        public void SaveTo_MergeMode_NullUpdateAction_ShouldThrow()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Merge,
                s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("updateAction");
        }

        [Fact]
        public void SaveTo_AppendMode_NullUpdateAction_ShouldNotThrow()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                s => s.Id, t => t.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().NotThrow();
        }

        [Fact]
        public void SaveTo_Expression_NullSourceKeyExp_ShouldThrow()
        {
            var source = new List<SourceItem> { new() { Id = 1, Name = "A" } };
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                (Expression<Func<SourceItem, int>>)null,
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("sourceKeyExp");
        }

        [Fact]
        public void SaveTo_Expression_NullSource_ShouldThrow()
        {
            ICollection<SourceItem> source = null!;
            var target = new List<TargetItem>();

            Action act = () => source.SaveTo(target, CollectionSaveMode.Append,
                (Expression<Func<SourceItem, int>>)(s => s.Id),
                s => new TargetItem { Id = s.Id, Name = s.Name }, null);

            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("source");
        }

        #endregion

        #region Cache key collision verification

        [Fact]
        public void SaveTo_Expression_TwoDifferentSourceTypes_ShouldNotReturnWrongCachedDelegate()
        {
            // This test verifies the cache key fix: two different source types with the
            // same lambda text must not collide and return each other's compiled delegate.
            var sourceA = new List<SourceItem> { new() { Id = 7, Name = "A7" } };
            var sourceB = new List<SourceItem2> { new() { Id = 8, Name = "B8" } };
            var targetA = new List<TargetItem>();
            var targetB = new List<TargetItem>();

            sourceA.AppendTo(targetA, s => s.Id, s => new TargetItem { Id = s.Id, Name = s.Name });
            sourceB.AppendTo(targetB, s => s.Id, s => new TargetItem { Id = s.Id, Name = s.Name });

            targetA.Single().Id.Should().Be(7);
            targetA.Single().Name.Should().Be("A7");
            targetB.Single().Id.Should().Be(8);
            targetB.Single().Name.Should().Be("B8");
        }

        [Fact]
        public void SaveTo_Expression_TargetKeySelector_MapsByMemberName()
        {
            // The expression replacer maps source member by name to target member.
            // Both SourceItem and TargetItem have "Id" property.
            var source = new List<SourceItem>
            {
                new() { Id = 1, Name = "NewA" },
                new() { Id = 2, Name = "B" }
            };
            var target = new List<TargetItem> { new() { Id = 1, Name = "OldA" } };

            source.MergeTo(target, s => s.Id,
                s => new TargetItem { Id = s.Id, Name = s.Name },
                (s, t) => t.Name = s.Name);

            target.Select(t => t.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            target.Single(t => t.Id == 1).Name.Should().Be("NewA");
        }

        #endregion

        #region Helper types

        public class SourceItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class SourceItem2
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TargetItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}
