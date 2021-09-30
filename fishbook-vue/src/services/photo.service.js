import ApiService from "./api.service";
import SiteUrl from "../settings/siteUrl.settings";

class PhotoService {
  async upload(data) {
    let body = new FormData();

    Object.keys(data).forEach((k) => body.append(k, data[k]));

    let r = await ApiService.post(SiteUrl.upload(), body);

    return r;
  }

  async update(id, data) {
    let url = `${SiteUrl.photos()}/${id}/update`;

    let r = await ApiService.put(url, data);
    return r;
  }

  async delete(id) {
    let url = `${SiteUrl.photos()}/${id}/delete`;

    let r = await ApiService.delete(url);
    return r;
  }
}

export default new PhotoService();
