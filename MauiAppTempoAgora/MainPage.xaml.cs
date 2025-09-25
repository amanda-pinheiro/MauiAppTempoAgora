using MauiAppTempoAgora.Models;
using MauiAppTempoAgora.Services;
using System;

namespace MauiAppTempoAgora
{
    public partial class MainPage : ContentPage
    {
  
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked_Previsao(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txt_cidade.Text))
                {
                    lbl_res.Text = "Preencha a cidade.";
                    return;
                }

                string cidade = txt_cidade.Text;
                string chave = "ce5ac41c46a5e251d991e398f400fb17";
                string url = $"https://api.openweathermap.org/data/2.5/weather?" +
                            $"q={cidade}&units=metric&lang=pt_br&appid={chave}";

                using (HttpClient cliente = new HttpClient())
                {
                    using (HttpResponseMessage response = await cliente.GetAsync(url))
                    {
                        // Verificação se a cidade não foi encontrada
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            await DisplayAlert("Cidade não encontrada",
                                $"A cidade '{cidade}' não foi encontrada. " +
                                $"Verifique o nome e tente novamente.",
                                "OK");
                            return;
                        }

                        string json = await response.Content.ReadAsStringAsync();

                        // Usa o seu DataService existente
                        Tempo? t = await DataService.GetPrevisao(txt_cidade.Text);
                        if (t != null)
                        {
                            string dados_previsao = "";

                            dados_previsao = $"Latitude: {t.lat} \n" +
                                           $"Longitude: {t.lon} \n" +
                                           $"Nascer do Sol: {t.sunrise} \n" +
                                           $"Por do Sol: {t.sunset} \n" +
                                           $"Temp Máx: {t.temp_max} \n" +
                                           $"Temp Min: {t.temp_min} \n" +
                                           $"Como está o dia: {t.description} \n" +
                                           $"Velocidadade do vento: {t.speed} km/h \n" +
                                           $"Visibilidade: {t.visibility} km";
                            
                            lbl_res.Text = dados_previsao;

                            string mapa = $"https://embed.windy.com/embed.html?" +
                                $"type=map&location=coordinates&metricRain=mm&metricTemp=°C" +
                                $"&metricWind=km/h&zoom=5&overlay=wind&product=ecmwf&level=surface" +
                                $"&lat={t.lat.ToString().Replace(",", ".")}&lon={t.lon.ToString().Replace(",", ".")}";

                            wv_mapa.Source = mapa;
                        }
                        else
                        {
                            lbl_res.Text = "Sem dados de Previsão";
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Verifica se é problema de internet
                if (ex.Message.Contains("No such host is known") ||
                    ex.Message.Contains("Name or service not known") ||
                    ex.Message.Contains("network is unreachable"))
                {
                    await DisplayAlert("Sem Internet",
                        "Você está sem conexão com a internet. Verifique sua conexão e tente novamente.",
                        "OK");
                }
                else
                {
                    await DisplayAlert("Erro de Conexão", $"Problema na conexão: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ops", ex.Message, "OK");
            }
        }

        private async void Button_Clicked_Localizacao(object sender, EventArgs e)
        {
            try
            {
                GeolocationRequest request = 
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                Location? local = await Geolocation.Default.GetLocationAsync(request);


                if (local != null)
                {
                    string local_disp = $"Latitude: {local.Latitude} \n" +
                                        $"Longitude: {local.Longitude}";

                    lbl_coords.Text = local_disp;

                    GetCidade(local.Latitude, local.Longitude);
                }
                else 
                {
                    lbl_coords.Text = "Nenhuma localização";
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Erro: Dispositivo não suporta", fnsEx.Message, "OK");

            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Erro: Localização Desabilitada", fneEx.Message, "OK");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Erro: Permissão da Localização", pEx.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro!", ex.Message, "OK");
            }


        }

        private async void GetCidade(double lat, double lon)
        {
            try
            {
                IEnumerable<Placemark> places = await Geocoding.Default.GetPlacemarksAsync(lat, lon);

                Placemark? place = places.FirstOrDefault();

                if (place != null)
                {
                    txt_cidade.Text = place.Locality;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro: Obtenção do nome da Cidade", ex.Message, "OK");
            }
        }
    }
}