using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitBranchesViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public IReadOnlyReactiveList<BranchItemViewModel> Branches { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public CommitBranchesViewModel(ISessionService applicationService)
        {
            Title = "Commit Branch";

            var branches = new ReactiveList<Octokit.Branch>();
            Branches = branches.CreateDerivedCollection(
                x => new BranchItemViewModel(x.Name, () => {
                    var vm = this.CreateViewModel<CommitsViewModel>();
                    NavigateTo(vm.Init(RepositoryOwner, RepositoryName, x.Name));
                }),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
                branches.Reset(await applicationService.GitHubClient.Repository.GetAllBranches(RepositoryOwner, RepositoryName)));
        }

        public CommitBranchesViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}

