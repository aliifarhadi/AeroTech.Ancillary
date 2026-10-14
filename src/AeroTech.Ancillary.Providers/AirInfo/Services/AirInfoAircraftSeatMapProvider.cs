using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers.AirInfo.Wire;

namespace AeroTech.Ancillary.Providers.AirInfo.Services
{
    public sealed class AirInfoAircraftSeatMapProvider : IAircraftSeatMapProvider
    {
        private const string SeatMapRoute = "v1/Aircrafts/{0}/DisplaySeatMap";

        private readonly HttpClient _httpClient;

        public AirInfoAircraftSeatMapProvider(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<AircraftSeatMap> GetAsync(int aircraftId, CancellationToken cancellationToken = default)
        {
            if (_httpClient.BaseAddress is null)
                throw ExceptionFactory.AircraftSeatMapCouldNotBeRetrieved(aircraftId, "the provider address is not configured");

            try
            {
                using var response = await _httpClient.GetAsync(string.Format(SeatMapRoute, aircraftId), cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw ExceptionFactory.AircraftSeatMapCouldNotBeRetrieved(aircraftId, $"status {(int)response.StatusCode}");

                var envelope = await response.Content.ReadFromJsonAsync<AirInfoEnvelope<AirInfoSeatMap>>(AirInfoJson.Options, cancellationToken);

                return envelope?.Data is { } seatMap
                    ? ToSeatMap(seatMap)
                    : throw ExceptionFactory.AircraftSeatMapCouldNotBeRetrieved(aircraftId, "the answer holds no seat map");
            }
            catch (HttpRequestException)
            {
                throw ExceptionFactory.AircraftSeatMapCouldNotBeRetrieved(aircraftId, "the provider is unreachable");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw ExceptionFactory.AircraftSeatMapCouldNotBeRetrieved(aircraftId, "the provider did not answer in time");
            }
            catch (JsonException)
            {
                throw ExceptionFactory.AircraftSeatMapCouldNotBeRetrieved(aircraftId, "the answer is not readable");
            }
        }

        private static AircraftSeatMap ToSeatMap(AirInfoSeatMap seatMap)
            => new(
                (seatMap.Decks ?? []).Select(deck => new AircraftDeck(
                        deck.DeckType,
                        ToConfiguration(deck.DeckConfiguration),
                        (deck.Cabins ?? []).Select(cabin => new AircraftCabin(
                                cabin.CabinClass,
                                cabin.CabinClassId > 0 ? cabin.CabinClassId : null,
                                cabin.CabinStartSeatRow,
                                cabin.CabinEndSeatRow,
                                (cabin.Items ?? []).Select(item => new AircraftCabinItem(
                                        item.Number,
                                        item.Code,
                                        item.Column,
                                        item.Row,
                                        item.Title,
                                        item.CharacteristicsCodes ?? [],
                                        item.Conditions ?? [],
                                        item.Coordinates?.X ?? 0,
                                        item.Coordinates?.Y ?? 0,
                                        item.Coordinates?.Length ?? 0))
                                    .ToList()))
                            .ToList()))
                    .ToList(),
                seatMap.Dictionaries?.Facilities ?? [],
                seatMap.Dictionaries?.SeatCharacteristics ?? []);

        private static AircraftDeckConfiguration ToConfiguration(AirInfoDeckConfiguration? configuration)
            => configuration is null
                ? new AircraftDeckConfiguration(0, 0, 0, 0, 0, 0, 0, 0, [])
                : new AircraftDeckConfiguration(
                    configuration.Width,
                    configuration.Length,
                    configuration.StartSeatRow,
                    configuration.EndSeatRow,
                    configuration.StartWingsX,
                    configuration.EndWingsX,
                    configuration.StartWingsRow,
                    configuration.EndWingsRow,
                    configuration.ExitRowsX ?? []);
    }
}
