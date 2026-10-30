using System.ComponentModel.DataAnnotations.Schema;
using GLMS.Web.Patterns.State;
using System.ComponentModel.DataAnnotations;
using GLMS.Web.Patterns.Observer;

namespace GLMS.Web.Models
{
    public class Contract
    {
        public int ContractId { get; set; }

        [Display(Name = "Client")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [DataType(DataType.Date), Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date), Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required, StringLength(50), Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        [Display(Name = "Signed Agreement")]
        public string? SignedAgreementPath { get; set; }

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        // ---- State pattern ----
        [NotMapped]
        public IContractState State => ContractStateFactory.FromStatus(Status);

        public bool CanRaiseServiceRequest() => State.CanRaiseServiceRequest();

        public void Activate() => State.Activate(this);

        public void PutOnHold() => State.PutOnHold(this);

        public void Expire() => State.Expire(this);

        public void SetState(IContractState newState)
        {
            var oldStatus = Status;
            Status = ContractStateFactory.ToStatus(newState);

            if (oldStatus != Status)
            {
                NotifyObservers(oldStatus.ToString());
            }
        }

        // ---- Observer pattern ----
        private readonly List<IContractObserver> _observers = new();

        public void Attach(IContractObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IContractObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers(string oldStatus)
        {
            foreach (var observer in _observers)
            {
                observer.Update(this, oldStatus);
            }
        }
    }
}