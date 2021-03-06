using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Writely.Exceptions;
using Writely.Models;
using Writely.Repositories;
using Xunit;

namespace Writely.UnitTests.Repositories
{
    public class EntryRepositoryTest : DatabaseTestBase
    {
        [Fact]
        public async Task GetById_EntryFound_ReturnsEntry()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(1);
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetById(entries[0].Id);

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(entries[0].Id);
        }

        [Fact]
        public async Task GetById_EntryNotFound_ReturnsNull()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetById(1L);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAll_ReturnsEntries()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(3);
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetAll() as List<Entry>;

            // Assert
            result?.Count.Should().Be(entries.Count);
        }

        [Fact]
        public async Task GetAll_JournalNotFound_ThrowsJournalNotFoundException()
        {
            // Arrange
            await PrepareDatabase();
            var repo = GetEntryRepo(null!);

            // Assert
            repo.Invoking(r => r.GetAll())
                .Should()
                .Throw<JournalNotFoundException>();
        }

        [Fact]
        public async Task GetAll_CanSetOrderOfSequence_ReturnsOrderedEntries()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(3);
            var alphabets = "alphabets";
            var alphabits = "alphabits";
            var aardvarks = "aardvarks";
            entries[0].Title = alphabits;
            entries[1].Title = aardvarks;
            entries[2].Title = alphabets;
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);
            
            // Act
            var result = await repo.GetAll(new QueryFilter { OrderBy = SortOrder.Descending }) as List<Entry>;

            // Assert
            result!.Should().BeInDescendingOrder(e => e.Title);
        }

        [Fact]
        public async Task GetAll_CanLimitNumberOfEntries_ReturnsLimitedEntries()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(10);
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetAll(new QueryFilter { Limit = 4 });

            // Assert
            result?.Count().Should().Be(4);
        }

        [Fact]
        public async Task GetAll_NegativeLimit_ReturnsAllEntries()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            journal.Entries = Helpers.GetEntries(5);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetAll(new QueryFilter { Limit = -4 });

            // Assert
            result?.Count().Should().Be(5);
        }

        [Fact]
        public async Task GetAllByTag_JournalFound_ReturnsMatchingEntries()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(5);
            entries[1].Tags = "dogs,cats";
            entries[2].Tags = "cats";
            entries[4].Tags = "frogs,dogs,cats,chickens";
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetAllByTag(new QueryFilter { Tags = "cats" }) as List<Entry>;

            // Assert
            result?.Count.Should().Be(3);
            result?.ForEach(e 
                => e.GetTags()?.Contains("cats").Should().BeTrue());
        }

        [Fact]
        public async Task GetAllByTag_JournalFound_SortsByTitleDescending_ReturnsSortedEntriesByTag()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(5);
            var tag = "funnytag";
            SetTitleAndTags(entries[0], "x", tag);
            SetTitleAndTags(entries[1], "y", tag);
            SetTitleAndTags(entries[2], "z", tag);
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetAllByTag(new QueryFilter { Tags = tag, OrderBy = SortOrder.Descending } ) as List<Entry>;

            // Assert
            result!.Count.Should().Be(3);
            result!.Should().BeInDescendingOrder(e => e.Title);
        }

        [Fact]
        public async Task GetAllByTag_JournalFound_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(3);
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.GetAllByTag(new QueryFilter { Tags = "blah" }) as List<Entry>;

            // Assert
            result?.Count.Should().Be(0);
        }

        [Fact]
        public async Task GetAllByTag_JournalNotFound_ThrowsJournalNotFoundException()
        {
            // Arrange
            await PrepareDatabase();
            
            var repo = GetEntryRepo(1L);

            // Assert
            repo.Invoking(r => r.GetAllByTag(new QueryFilter { Tags = "Dogs" }))
                .Should()
                .Throw<JournalNotFoundException>();
        }

        [Fact]
        public async Task GetAllByTag_JournalFound_TagsEmpty_ThrowsEmptyTagsException()
        {
            // Arrange
            await PrepareDatabase();
            var repo = GetEntryRepo(1L);
            
            // Assert
            repo.Invoking(r => r.GetAllByTag(new QueryFilter()))
                .Should()
                .Throw<EmptyTagsException>();
        }

        [Fact]
        public async Task Find_EntryFound_ReturnsEntry()
        {
            // Arrange
            await PrepareDatabase();
            var journal = Helpers.GetJournal();
            var entries = Helpers.GetEntries(3);
            entries[0].Title = "Spiffy Title";
            Helpers.AddEntriesToJournal(journal, entries);
            await SaveJournal(journal);
            var repo = GetEntryRepo(journal.Id);

            // Act
            var result = await repo.Find(e => e.Title!.Contains("Spiffy"));

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be(entries[0].Title);
        }

        [Fact]
        public async Task Find_EntryNotFound_ReturnsNull()
        {
            // Arrange
            await PrepareDatabase();
            var repo = GetEntryRepo(3L);

            // Act
            var result = await repo.Find(e => e.Id == 1L);

            // Assert
            result.Should().BeNull();
        }

        private EntryRepository GetEntryRepo(long? journalId) => new EntryRepository(Context!, journalId);

        private void SetTitleAndTags(Entry entry, string title, string tags)
        {
            entry.Title = title;
            entry.Tags = tags;
        }
    }
}