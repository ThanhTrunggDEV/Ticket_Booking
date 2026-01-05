# Round-Trip Flight Search Feature

## Overview
The round-trip flight search feature allows users to search for flights with both outbound and return flight legs. This feature is essential for a booking system, enabling customers to plan complete round-trip journeys with departure and return dates.

## Feature Architecture

### 1. User Interface Layer

#### Location: `Views/User/Index.cshtml`

**Trip Type Selection Section:**
```html
<!-- Trip Type Selection (Top-level, prominent display) -->
<div class="mb-8 pb-6 border-b border-gray-100">
    <label>Trip Type</label>
    <div class="flex gap-3">
        <!-- One Way Radio -->
        <label class="flex items-center gap-3 p-3 px-5 bg-indigo-50 border-2 border-indigo-200 rounded-xl cursor-pointer flex-1 hover:bg-indigo-100 transition-all duration-200 group" id="oneWayLabel">
            <input type="radio" name="tripType" value="OneWay" class="w-5 h-5 text-indigo-600 cursor-pointer" onchange="toggleReturnDate()" />
            <span class="text-sm font-bold text-gray-900">One Way</span>
        </label>
        
        <!-- Round Trip Radio -->
        <label class="flex items-center gap-3 p-3 px-5 bg-gray-50 border-2 border-gray-200 rounded-xl cursor-pointer flex-1 hover:border-indigo-300 transition-all duration-200 group" id="roundTripLabel">
            <input type="radio" name="tripType" value="RoundTrip" class="w-5 h-5 text-indigo-600 cursor-pointer" onchange="toggleReturnDate()" />
            <span class="text-sm font-bold text-gray-900">Round Trip</span>
        </label>
    </div>
</div>
```

**Key Features:**
- Radio button selection between "One Way" and "Round Trip"
- Enhanced visual styling with colored backgrounds and hover effects
- Triggers `toggleReturnDate()` JavaScript function on change

**Return Date Field:**
```html
<!-- Return Date (Conditionally displayed) -->
<div id="returnDateContainer" style="display: @(Model.TripType == "RoundTrip" ? "block" : "none");" class="transition-all duration-300">
    <label class="block text-xs font-bold text-gray-400 uppercase tracking-widest mb-2">Return Date</label>
    <input type="date" name="returnDate" value="@(Model.ReturnDate?.ToString("yyyy-MM-dd"))" class="w-full bg-gray-50 text-gray-900 border-2 border-gray-100 rounded-xl px-4 py-3 font-semibold focus:outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 hover:border-indigo-300 transition-colors" />
</div>
```

**Key Features:**
- Conditionally visible based on trip type selection
- Initially hidden for one-way trips
- Shown in 5-column responsive grid alongside departure date
- Smooth CSS transitions for visibility changes

#### JavaScript Toggle Logic

```javascript
function toggleReturnDate() {
    const tripType = document.querySelector('input[name="tripType"]:checked').value;
    const returnDateContainer = document.getElementById('returnDateContainer');
    const returnDateInput = document.querySelector('input[name="returnDate"]');
    
    if (tripType === 'RoundTrip') {
        returnDateContainer.style.display = 'block';
        returnDateInput.required = true;
    } else {
        returnDateContainer.style.display = 'none';
        returnDateInput.required = false;
        returnDateInput.value = '';
    }
}
```

**Functionality:**
- Detects selected trip type
- Shows/hides return date input container
- Makes return date required for round-trip searches
- Clears return date value when switching to one-way
- Handles form validation state

### 2. View Model Layer

#### Location: `Models/ViewModels/SearchTripViewModel.cs`

```csharp
public class SearchTripViewModel
{
    public string? FromCity { get; set; }
    public string? ToCity { get; set; }
    public DateTime? Date { get; set; }                           // Departure date
    public DateTime? ReturnDate { get; set; }                    // Return date (round-trip only)
    public string TripType { get; set; } = "OneWay";             // "OneWay" or "RoundTrip"
    public SortCriteria SortBy { get; set; } = SortCriteria.DepartureTimeAsc;
    public SeatClass SeatClass { get; set; } = SeatClass.Economy;
    public IEnumerable<Trip> Trips { get; set; } = new List<Trip>();
    public IEnumerable<string> AvailableCities { get; set; } = new List<string>();
}
```

**Key Properties:**
- `ReturnDate`: Stores the return flight date (nullable, only populated for round-trip searches)
- `TripType`: Indicates search type ("OneWay" or "RoundTrip")
- Both date properties are passed through the search form and back to the view for form persistence

### 3. Controller Layer

#### Location: `Controllers/UserController.cs`

**Index Action Signature:**
```csharp
public async Task<IActionResult> Index(
    string? fromCity, 
    string? toCity, 
    DateTime? date,           // Departure date
    DateTime? returnDate,     // Return date (round-trip only)
    string tripType = "OneWay",
    SortCriteria sortBy = SortCriteria.DepartureTimeAsc, 
    SeatClass seatClass = SeatClass.Economy)
```

**Current Implementation Note:**
- The controller receives `tripType` and `returnDate` parameters
- Currently uses departure `date` only for filtering trips
- Return date is stored in ViewModel but NOT actively used in filtering logic

**Search Logic:**
```csharp
// If search criteria provided, use SearchAndSortTripsAsync
if (!string.IsNullOrEmpty(fromCity) && !string.IsNullOrEmpty(toCity))
{
    trips = await _tripRepositoryConcrete.SearchAndSortTripsAsync(fromCity, toCity, date, sortBy, seatClass);
    trips = trips.Where(t => t.DepartureTime > DateTime.Now);
}
```

**Current Behavior:**
- Filters trips by: `FromCity`, `ToCity`, departure `Date`, `SortBy`, `SeatClass`
- Does NOT filter return trips by return date
- Displays same outbound trip results regardless of return date selection

### 4. Repository Layer

#### Location: `Repositories/TripRepository.cs`

**Method:** `SearchAndSortTripsAsync(fromCity, toCity, date, sortBy, seatClass)`

**Parameters:**
- `fromCity`: Origin airport/city
- `toCity`: Destination airport/city
- `date`: Departure date filter
- `sortBy`: Sort criteria (8 options available)
- `seatClass`: Seat class filter (Economy, Business, First Class)

**Note:**
- Currently does NOT accept or use `returnDate` parameter
- Would need modification to support round-trip return flight filtering

### 5. Localization Support

#### Files Modified: `Resources/SharedResource*.resx`

**Localized Strings Added:**
- `User.TripType` - "Trip Type" / "Loại chuyến bay"
- `User.OneWay` - "One Way" / "Một chiều"
- `User.RoundTrip` - "Round Trip" / "Hai chiều"
- `User.ReturnDate` - "Return Date" / "Ngày về"

**Languages Supported:**
- English (en)
- Vietnamese (vi)
- Default language

## User Experience Flow

### One-Way Search Flow:
1. User selects "One Way" radio button
2. Return date field is hidden
3. User enters: From City, To City, Departure Date
4. User clicks Search
5. Results show outbound flights for selected date

### Round-Trip Search Flow (Current):
1. User selects "Round Trip" radio button
2. Return date field becomes visible and required
3. User enters: From City, To City, Departure Date, Return Date
4. User clicks Search
5. Results show outbound flights for departure date
6. ⚠️ **Note:** Return date is currently NOT used for filtering - user must manually choose return flights

## Technical Considerations

### Current Limitations

1. **Return Flight Filtering Not Implemented**
   - The system accepts `ReturnDate` parameter but doesn't use it for filtering
   - Users must search separately for return flights
   - Could be enhanced to show paired outbound/return suggestions

2. **Trip Display**
   - Currently shows individual one-way trips
   - Does not automatically group outbound + return flights
   - Would need booking logic enhancement for round-trip packages

3. **Database Structure**
   - Current `Trip` model represents single legs
   - Round-trip tickets would need to combine two trips
   - May need `RoundTripTicket` or composite ticket concept

### Future Enhancement Opportunities

1. **Smart Return Flight Suggestions**
   - When round-trip selected with return date, filter return flights automatically
   - Show paired flight recommendations

2. **Round-Trip Pricing**
   - Implement discount logic for round-trip bookings
   - Add per-trip vs. round-trip pricing comparison

3. **Multi-Leg Search**
   - Support complex routing (e.g., A→B→C)
   - Handle connections and layovers

4. **Booking Integration**
   - Create composite round-trip bookings
   - Link outbound and return ticket reservations
   - Handle cancellation policies for paired trips

## Testing Checklist

- [x] Trip type selection toggles return date field
- [x] Return date field shows only for round-trip
- [x] Return date cleared when switching to one-way
- [x] Form values persist after search
- [x] Multilingual labels display correctly
- [x] Responsive design works on all screen sizes
- [ ] Return date filtering implemented
- [ ] Round-trip pricing applied
- [ ] Return flights matched to outbound selection

## Summary

The round-trip search feature provides the user interface and data model for round-trip flight bookings, with full multilingual support and conditional UI display. The core search logic accepts round-trip parameters but currently implements only one-way flight filtering. The feature is ready for backend enhancement to support complete round-trip search functionality with return flight matching and pricing.
